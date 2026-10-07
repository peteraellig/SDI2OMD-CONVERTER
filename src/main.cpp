#include <Windows.h>
#include <atlbase.h>
#import "C:\Program Files\Blackmagic Design\Blackmagic Desktop Video\DeckLinkAPI64.dll" raw_interfaces_only raw_native_types no_namespace named_guids
#include "libomt.h"
#include "low_latency_queue.h"
#include <limits>
#include <atomic>
#include <chrono>
#include <condition_variable>
#include <deque>
#include <iostream>
#include <mutex>
#include <stdexcept>
#include <string>
#include <thread>
#include <vector>

void check(HRESULT hr, const char* action) {
    if (FAILED(hr)) { char text[160]; sprintf_s(text, "%s failed (HRESULT 0x%08lX)", action, hr); throw std::runtime_error(text); }
}
struct ComScope { ComScope(){check(CoInitializeEx(nullptr, COINIT_MULTITHREADED), "COM");} ~ComScope(){CoUninitialize();} };
std::atomic<bool> running{true};
std::atomic<bool> diagnostics{true};
BOOL WINAPI consoleHandler(DWORD event) { if(event == CTRL_C_EVENT || event == CTRL_BREAK_EVENT){running=false;return TRUE;}return FALSE; }
std::string utf8(BSTR text) {
    if(!text)return {};
    int length=WideCharToMultiByte(CP_UTF8,0,text,SysStringLen(text),nullptr,0,nullptr,nullptr);
    std::string result(length,'\0'); WideCharToMultiByte(CP_UTF8,0,text,SysStringLen(text),result.data(),length,nullptr,nullptr);return result;
}
std::vector<CComPtr<IDeckLink>> devices(){
    CComPtr<IDeckLinkIterator> iterator;
    check(iterator.CoCreateInstance(__uuidof(CDeckLinkIterator)), "DeckLink iterator");
    std::vector<CComPtr<IDeckLink>> result;
    for(;;){ CComPtr<IDeckLink> device; if(iterator->Next(&device)!=S_OK)break; result.push_back(device); }
    return result;
}
struct Sender {
    omt_send_t* handle;
    Sender(const std::string& name, OMTQuality quality=OMTQuality_Default):handle(omt_send_create(name.c_str(),quality)){if(!handle)throw std::runtime_error("OMT sender creation failed");}
    ~Sender(){omt_send_destroy(handle);}
};
struct AudioRoute { int left; int right; };
AudioRoute parseAudioRoute(const std::string& name){
    if(name=="Stereo 1&2")return {0,1};
    if(name=="Stereo 3&4")return {2,3};
    if(name=="Stereo 5&6")return {4,5};
    if(name=="Stereo 7&8")return {6,7};
    if(name.size()==6 && name.starts_with("Mono ") && name[5]>='1' && name[5]<='8'){
        int channel=name[5]-'1';return {channel,channel};
    }
    throw std::runtime_error("Invalid audio selection; choose Stereo 1&2, 3&4, 5&6, 7&8 or Mono 1..8");
}
std::vector<float> routeAudio(const int32_t* samples,int count,AudioRoute route){
    std::vector<float> output(static_cast<size_t>(count)*2);
    for(int i=0;i<count;++i){
        output[i]=static_cast<float>(samples[i*8+route.left]/2147483648.0);
        output[count+i]=static_cast<float>(samples[i*8+route.right]/2147483648.0);
    }
    return output;
}
struct Packet { OMTMediaFrame frame{}; std::vector<unsigned char> video; std::vector<float> audio; };
class Capture : public IDeckLinkInputCallback_v14_2_1 {
    std::atomic<ULONG> refs{1};
    LowLatencyQueue<Packet> queue;
    Sender& sender; AudioRoute audioRoute; int frameRateN, frameRateD; OMTVideoFlags videoFlags;
    std::mutex failureMutex; std::string failure;
    std::atomic<int64_t> previousVideo{-1};
public:
    std::atomic<uint64_t> captured{0}, sent{0}, captureGaps{0}, noSignal{0}, noDelivery{0}, errors{0}, sendErrors{0};
    std::atomic<bool> signalPresent{false};
    std::atomic<int64_t> lastVideoArrival{0}, lastSendFinished{0}, encodingSince{0};
    std::atomic<int> lastEncodeMs{0};
    std::atomic<bool> failed{false};
private:
    std::thread worker;
public:
    static int64_t nowMs(){return std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::steady_clock::now().time_since_epoch()).count();}
    Capture(Sender& value, int rateN, int rateD, AudioRoute route, OMTVideoFlags flags):
        queue(std::chrono::microseconds((2000000ll*rateD+rateN-1)/rateN)),sender(value),audioRoute(route),frameRateN(rateN),frameRateD(rateD),videoFlags(flags){
        // Start only after all counters and state have been constructed.
        worker=std::thread([this]{try {
        ComScope scope;
        while(auto next=queue.wait()){Packet packet=std::move(*next);
            packet.frame.Data=packet.frame.Type==OMTFrameType_Video ? static_cast<void*>(packet.video.data()) : static_cast<void*>(packet.audio.data());
            auto begin=nowMs();encodingSince.store(begin,std::memory_order_relaxed);
            auto delivered=omt_send(sender.handle,&packet.frame);
            auto end=nowMs();lastEncodeMs.store(static_cast<int>(end-begin),std::memory_order_relaxed);
            lastSendFinished.store(end,std::memory_order_relaxed);encodingSince.store(0,std::memory_order_relaxed);
            if(delivered>0){if(packet.frame.Type==OMTFrameType_Video)++sent;}
            else if(delivered==0)++noDelivery; // No receiver or silent audio is not an error.
            else {++sendErrors;throw std::runtime_error("OMT send failed; transmission stopped");}
        }
        }catch(const std::exception& e){fail(e.what());}catch(...){fail("Unexpected encoder failure; transmission stopped");}});
    }
    ~Capture(){finish();}
    void fail(const std::string& message){{std::lock_guard lock(failureMutex);failure=message;}failed=true;queue.close();running=false;}
    std::string failureMessage(){std::lock_guard lock(failureMutex);return failure;}
    void finish(){queue.close();if(worker.joinable())worker.join();}
    auto queueStats(){return queue.stats();}
    void push(Packet&& packet){bool video=packet.frame.Type==OMTFrameType_Video;auto timestamp=packet.frame.Timestamp;auto samples=packet.frame.SamplesPerChannel;queue.push(std::move(packet),video,timestamp,samples);}
    HRESULT STDMETHODCALLTYPE QueryInterface(REFIID iid, void** result) override {
        if(!result)return E_POINTER;*result=nullptr;
        if(iid==IID_IUnknown||iid==__uuidof(IDeckLinkInputCallback_v14_2_1)){*result=static_cast<IDeckLinkInputCallback_v14_2_1*>(this);AddRef();return S_OK;}return E_NOINTERFACE;
    }
    ULONG STDMETHODCALLTYPE AddRef() override{return ++refs;}
    ULONG STDMETHODCALLTYPE Release() override{auto value=--refs;if(!value)delete this;return value;}
    HRESULT STDMETHODCALLTYPE VideoInputFormatChanged(_BMDVideoInputFormatChangedEvents, IDeckLinkDisplayMode*, _BMDDetectedVideoInputFormatFlags) override{return S_OK;}
    HRESULT STDMETHODCALLTYPE VideoInputFrameArrived(IDeckLinkVideoInputFrame_v14_2_1* video, IDeckLinkAudioInputPacket* audio) override {
        try {
            if(failed)return S_OK;
            const bool validSignal=video&&!(video->GetFlags()&bmdFrameHasNoInputSource);
            if(video)signalPresent.store(validSignal,std::memory_order_relaxed);
            if(validSignal){try {
                void* bytes=nullptr;__int64 timestamp=0,duration=0;
                if(SUCCEEDED(video->GetBytes(&bytes))&&bytes&&SUCCEEDED(video->GetStreamTime(&timestamp,&duration,10000000))&&duration>0){
                    Packet p;p.frame.Type=OMTFrameType_Video;p.frame.Codec=OMTCodec_UYVY;
                    p.frame.Width=video->GetWidth();p.frame.Height=video->GetHeight();p.frame.Stride=video->GetRowBytes();
                    if(p.frame.Width<=0||p.frame.Width>4096||p.frame.Height<=0||p.frame.Height>2160||p.frame.Stride<p.frame.Width*2||p.frame.Stride>std::numeric_limits<int>::max()/p.frame.Height)
                        throw std::runtime_error("Invalid captured video buffer");
                    p.frame.Flags=videoFlags;p.frame.FrameRateN=frameRateN;p.frame.FrameRateD=frameRateD;p.frame.Timestamp=timestamp;p.frame.AspectRatio=16.0f/9;p.frame.ColorSpace=OMTColorSpace_BT709;
                    p.frame.DataLength=p.frame.Stride*p.frame.Height;
                    auto previous=previousVideo.exchange(timestamp,std::memory_order_relaxed);
                    if(previous>=0&&timestamp>previous+duration+duration/2)captureGaps+=static_cast<uint64_t>((timestamp-previous+duration/2)/duration-1);
                    auto first=static_cast<unsigned char*>(bytes);p.video.assign(first,first+p.frame.DataLength);++captured;
                    lastVideoArrival.store(nowMs(),std::memory_order_relaxed);push(std::move(p));
                }else ++errors;
            }catch(const std::bad_alloc&){throw;}catch(...){++errors;}
            }else if(video){++noSignal;previousVideo=-1;queue.clearVideo();}
            if(audio){void* bytes=nullptr;__int64 timestamp=0;auto count=audio->GetSampleFrameCount();
                if(count>0&&count<=48000&&SUCCEEDED(audio->GetBytes(&bytes))&&bytes&&SUCCEEDED(audio->GetPacketTime(&timestamp,10000000))){
                    // A large driver audio packet is trimmed from the front,
                    // preserving the freshest 120 ms and its original time.
                    int skipped=count>5760 ? static_cast<int>(count)-5760 : 0;
                    if(skipped){queue.discardAudioSamples(skipped);timestamp+=static_cast<int64_t>(skipped)*10000000/48000;count-=skipped;}
                    Packet p;p.frame.Type=OMTFrameType_Audio;p.frame.Codec=OMTCodec_FPA1;p.frame.Timestamp=timestamp;
                    p.frame.SampleRate=48000;p.frame.Channels=2;p.frame.SamplesPerChannel=count;p.frame.DataLength=count*2*4;
                    p.audio=routeAudio(static_cast<int32_t*>(bytes)+skipped*8,count,audioRoute);
                    push(std::move(p));
                }else if(count>0)++errors;
            }
        }catch(const std::bad_alloc&){++errors;fail("Capture ran out of memory; transmission stopped");}
        catch(...){++errors;}return S_OK;
    }
};
void listDevices(){auto all=devices();if(all.empty())throw std::runtime_error("No DeckLink devices found");
    for(size_t i=0;i<all.size();++i){CComBSTR name;all[i]->GetDisplayName(&name);CComPtr<IDeckLinkInput_v14_2_1> input;
        bool capture=SUCCEEDED(all[i]->QueryInterface(__uuidof(IDeckLinkInput_v14_2_1),reinterpret_cast<void**>(&input)));
        std::cout<<i<<": "<<utf8(name)<<" (capture interface: "<<(capture?"yes":"no")<<")\n";
    }
}
_BMDDisplayMode parseMode(const std::string& mode);
std::string jsonString(const std::string& text){
    std::string result="\"";const char hex[]="0123456789abcdef";
    for(unsigned char c:text){
        if(c=='"'||c=='\\'){result+='\\';result+=c;}
        else if(c<32){result+="\\u00";result+=hex[c>>4];result+=hex[c&15];}
        else result+=c;
    }return result+'"';
}
// Status queries never enable capture or change connector configuration.
void deviceStatus(){
    auto all=devices();std::cout<<'[';
    for(size_t i=0;i<all.size();++i){
        if(i)std::cout<<',';
        CComBSTR name;check(all[i]->GetDisplayName(&name),"Device name");
        CComPtr<IDeckLinkStatus> status;all[i]->QueryInterface(__uuidof(IDeckLinkStatus),reinterpret_cast<void**>(&status));
        long locked=0;__int64 busy=0,detected=0;
        auto lockResult=status?status->GetFlag(bmdDeckLinkStatusVideoInputSignalLocked,&locked):E_NOINTERFACE;
        auto busyResult=status?status->GetInt(bmdDeckLinkStatusBusy,&busy):E_NOINTERFACE;
        auto modeResult=status?status->GetInt(bmdDeckLinkStatusDetectedVideoInputMode,&detected):E_NOINTERFACE;
        std::string mode;
        if(lockResult==S_OK&&locked&&modeResult==S_OK&&detected!=bmdModeUnknown){
            CComPtr<IDeckLinkInput_v14_2_1> input;CComPtr<IDeckLinkDisplayMode> display;
            all[i]->QueryInterface(__uuidof(IDeckLinkInput_v14_2_1),reinterpret_cast<void**>(&input));
            if(input&&input->GetDisplayMode(static_cast<_BMDDisplayMode>(detected),&display)==S_OK&&display){
                CComBSTR modeName;if(display->GetName(&modeName)==S_OK)mode=utf8(modeName);
            }
        }
        std::cout<<"{\"index\":"<<i<<",\"name\":"<<jsonString(utf8(name))
            <<",\"signal\":"<<(lockResult==S_OK?(locked?"true":"false"):"null")
            <<",\"mode\":"<<(mode.empty()?"null":jsonString(mode))
            <<",\"busy\":"<<(busyResult==S_OK?std::to_string(busy):"null")<<'}';
    }std::cout<<"]\n";
}
OMTQuality parseQuality(const std::string& quality);
bool stopRequested();
void testOMT(const std::string& modeName="720p50",const std::string& qualityName="Normal",int frames=250,const std::string& audioName="Stereo 1&2",bool loopback=true,const std::string& senderName="SDI OMT Selftest"){
    auto audioRoute=parseAudioRoute(audioName);
    parseMode(modeName);int width=modeName.starts_with("1080")?1920:1280;int height=width==1920?1080:720;bool interlaced=modeName.starts_with("1080i");int fps=(modeName.ends_with("60")?60:50)/(interlaced?2:1);
    if((loopback&&frames<20)||frames<0||frames>1000)throw std::runtime_error("Selftest frame count must be 20..1000");
    Sender sender(senderName,parseQuality(qualityName));char address[1024]{};omt_send_getaddress(sender.handle,address,sizeof(address));
    auto receiver=loopback?omt_receive_create(address,static_cast<OMTFrameType>(OMTFrameType_Video|OMTFrameType_Audio),OMTPreferredVideoFormat_UYVY,OMTReceiveFlags_None):nullptr;
    if(loopback&&!receiver)throw std::runtime_error("OMT receiver creation failed");
    struct Cleanup{omt_receive_t* handle;~Cleanup(){if(handle)omt_receive_destroy(handle);}} cleanup{receiver};
    std::vector<unsigned char> pixels(width*height*2);for(size_t i=0;i<pixels.size();i+=4){
        // Distinct alternating scanlines prove both fields survive the interlaced codec path.
        unsigned char luma=interlaced&&((i/(width*2))%2)?160:80;
        pixels[i]=128;pixels[i+1]=luma;pixels[i+2]=128;pixels[i+3]=luma;
    }
    std::vector<int32_t> inputAudio((48000/fps)*8);
    for(int i=0;i<48000/fps;++i)for(int channel=0;channel<8;++channel)inputAudio[i*8+channel]=(channel+1)*134217728;
    auto audio=routeAudio(inputAudio.data(),48000/fps,audioRoute);
    OMTMediaFrame v{};v.Type=OMTFrameType_Video;v.Codec=OMTCodec_UYVY;v.Width=width;v.Height=height;v.Stride=width*2;v.Flags=interlaced?OMTVideoFlags_Interlaced:OMTVideoFlags_None;v.FrameRateN=fps;v.FrameRateD=1;v.AspectRatio=16.f/9;v.ColorSpace=OMTColorSpace_BT709;v.Data=pixels.data();v.DataLength=static_cast<int>(pixels.size());
    OMTMediaFrame a{};a.Type=OMTFrameType_Audio;a.Codec=OMTCodec_FPA1;a.SampleRate=48000;a.Channels=2;a.SamplesPerChannel=48000/fps;a.Data=audio.data();a.DataLength=static_cast<int>(audio.size()*4);
    int receivedVideo=0,receivedAudio=0;
    if(!loopback)std::cout<<"Sending "<<modeName<<" / "<<qualityName<<" / "<<audioName<<" as "<<address<<". Ctrl+C to stop.\n";
    for(int i=0;running&&(frames==0||i<frames);++i){if(!loopback&&stopRequested())break;v.Timestamp=a.Timestamp=static_cast<int64_t>(i)*10000000/fps;omt_send(sender.handle,&v);omt_send(sender.handle,&a);
        auto frame=loopback?omt_receive(receiver,OMTFrameType_Video,1):nullptr;
        if(frame){if(frame->Width!=width||frame->Height!=height||frame->Codec!=OMTCodec_UYVY||frame->FrameRateN!=fps||frame->FrameRateD!=1||frame->Flags!=v.Flags)throw std::runtime_error("Unexpected loopback video format");
            if(interlaced){
                if(!frame->Data||frame->Stride<width*2)throw std::runtime_error("Missing interlaced pixel data");
                auto data=static_cast<const unsigned char*>(frame->Data);
                for(int row=height/2;row<height/2+2;++row){int expected=(row%2)?160:80;
                    if(std::abs(static_cast<int>(data[row*frame->Stride+1])-expected)>4)throw std::runtime_error("Interlaced field/scanline order mismatch");}
            }
            ++receivedVideo;}
        frame=loopback?omt_receive(receiver,OMTFrameType_Audio,1):nullptr;if(frame){if(frame->Channels!=2||frame->SampleRate!=48000||frame->SamplesPerChannel<=0||!frame->Data)throw std::runtime_error("Unexpected loopback audio format");
            auto received=static_cast<float*>(frame->Data);
            const float expectedLeft=(audioRoute.left+1)/16.0f,expectedRight=(audioRoute.right+1)/16.0f;
            for(int sample=0;sample<frame->SamplesPerChannel;++sample)
                if(received[sample]!=expectedLeft||received[frame->SamplesPerChannel+sample]!=expectedRight)throw std::runtime_error("Loopback audio channel mapping mismatch");
            ++receivedAudio;}
        std::this_thread::sleep_for(std::chrono::microseconds(1000000/fps));
    }
    std::cout<<"OMT loopback: "<<receivedVideo<<" video frames, "<<receivedAudio<<" audio packets\n";
    if(loopback&&(receivedVideo<frames/2||receivedAudio<frames/2))throw std::runtime_error("Insufficient loopback frames");
}
bool stopRequested(){
    HANDLE handle=GetStdHandle(STD_INPUT_HANDLE);
    if(GetFileType(handle)!=FILE_TYPE_PIPE)return false;
    DWORD available=0;
    if(!PeekNamedPipe(handle,nullptr,0,nullptr,&available,nullptr))return true;
    if(!available)return false;
    char buffer[256];DWORD read=0;
    if(!ReadFile(handle,buffer,(available < sizeof(buffer) ? available : static_cast<DWORD>(sizeof(buffer))),&read,nullptr))return true;
    static std::string pending;
    pending.append(buffer,read);
    for(size_t end;(end=pending.find('\n'))!=std::string::npos;){
        auto command=pending.substr(0,end);pending.erase(0,end+1);
        if(command.starts_with("\xef\xbb\xbf"))command.erase(0,3);
        if(!command.empty()&&command.back()=='\r')command.pop_back();
        if(command=="stop")return true;
        if(command=="diagnostics on")diagnostics=true;
        if(command=="diagnostics off")diagnostics=false;
    }
    return false;
}
_BMDDisplayMode parseMode(const std::string& mode){
    if(mode=="720p50")return bmdModeHD720p50;
    if(mode=="720p60")return bmdModeHD720p60;
    if(mode=="1080p50")return bmdModeHD1080p50;
    if(mode=="1080p60")return bmdModeHD1080p6000;
    if(mode=="1080i50")return bmdModeHD1080i50;
    if(mode=="1080i60")return bmdModeHD1080i6000;
    throw std::runtime_error("Unsupported mode; choose 720p50, 720p60, 1080p50, 1080p60, 1080i50 or 1080i60");
}
OMTQuality parseQuality(const std::string& quality){
    if(quality=="High")return OMTQuality_High;
    if(quality=="Normal")return OMTQuality_Medium;
    if(quality=="Low")return OMTQuality_Low;
    throw std::runtime_error("Unsupported quality; choose High, Normal or Low");
}
void runCapture(int index,const std::string& name,int seconds,const std::string& modeName="720p50",const std::string& qualityName="Normal",const std::string& audioName="Stereo 1&2",bool reportDiagnostics=true){
    diagnostics=reportDiagnostics;
    auto audioRoute=parseAudioRoute(audioName);
    auto mode=parseMode(modeName);auto quality=parseQuality(qualityName);
    auto all=devices();if(index<0||index>=static_cast<int>(all.size()))throw std::runtime_error("Invalid device index; use --list");
    CComPtr<IDeckLinkInput_v14_2_1> input;check(all[index]->QueryInterface(__uuidof(IDeckLinkInput_v14_2_1),reinterpret_cast<void**>(&input)),"Capture interface");
    CComPtr<IDeckLinkDisplayMode> display;check(input->GetDisplayMode(mode,&display),"Get selected mode");
    __int64 duration=0,scale=0;check(display->GetFrameRate(&duration,&scale),"Get frame rate");
    if(duration<=0||scale<=0)throw std::runtime_error("Invalid display mode frame rate");
    long supported=0;_BMDDisplayMode actual=mode;
    check(input->DoesSupportVideoMode(bmdVideoConnectionUnspecified,mode,bmdFormat8BitYUV,bmdNoVideoInputConversion,bmdSupportedVideoModeDefault,&actual,&supported),"Check capture mode");
    if(!supported)throw std::runtime_error("Selected input does not support this mode in 8-bit YUV");
    Sender sender(name,quality);CComPtr<Capture> callback;callback.Attach(new Capture(sender,static_cast<int>(scale),static_cast<int>(duration),audioRoute,
        (display->GetFieldDominance()==bmdUpperFieldFirst||display->GetFieldDominance()==bmdLowerFieldFirst)?OMTVideoFlags_Interlaced:OMTVideoFlags_None));
    struct Stop{IDeckLinkInput_v14_2_1* input;~Stop(){input->StopStreams();input->SetCallback(nullptr);input->DisableAudioInput();input->DisableVideoInput();}} stop{input};
    check(input->EnableVideoInput(mode,bmdFormat8BitYUV,bmdVideoInputFlagDefault),"Enable selected capture mode (port may be occupied)");
    check(input->EnableAudioInput(bmdAudioSampleRate48kHz,bmdAudioSampleType32bitInteger,8),"Enable 8-channel SDI audio input");
    check(input->SetCallback(callback),"Set callback");check(input->StartStreams(),"Start streams");
    char address[1024]{};omt_send_getaddress(sender.handle,address,sizeof(address));
    std::cout<<"Sending "<<modeName<<" / "<<qualityName<<" / "<<audioName<<" as "<<address<<". Ctrl+C to stop.\n";
    const auto started=Capture::nowMs();
    for(int elapsed=0;running&&(seconds==0||elapsed<seconds);++elapsed){
        for(int tick=0;tick<10&&running;++tick){if(stopRequested()){running=false;break;}std::this_thread::sleep_for(std::chrono::milliseconds(100));}
        auto state=callback->queueStats();auto now=Capture::nowMs();
        auto videoArrival=callback->lastVideoArrival.load();auto sendFinished=callback->lastSendFinished.load();auto encoding=callback->encodingSince.load();
        auto connections=omt_send_connections(sender.handle);
        // One compact sample per second; no frame-by-frame logging or polling.
        std::cout<<"Health={\"uptimeMs\":"<<now-started<<",\"captured\":"<<callback->captured
            <<",\"sent\":"<<callback->sent<<",\"videoLost\":"<<state.videoDropped+callback->captureGaps.load()
            <<",\"audioLostSamples\":"<<state.audioSamplesDropped<<",\"errors\":"<<callback->errors.load()+callback->sendErrors.load()
            <<",\"signal\":"<<(callback->signalPresent?"true":"false")<<",\"videoAgeMs\":"<<(videoArrival?now-videoArrival:-1)
            <<",\"sendAgeMs\":"<<(sendFinished?now-sendFinished:-1)<<",\"sendBusyMs\":"<<(encoding?now-encoding:0)
            <<",\"encodeMs\":"<<callback->lastEncodeMs<<",\"audioQueueMs\":"<<state.audioMs
            <<",\"videoQueueMs\":"<<state.videoMs<<",\"connections\":"<<connections<<"}\n";
        if(diagnostics)std::cout<<"Captured="<<callback->captured<<" sent="<<callback->sent<<" queue drops="<<state.videoDropped
            <<" audio dropped samples="<<state.audioSamplesDropped<<" no signal="<<callback->noSignal<<" no delivery="<<callback->noDelivery
            <<" capture errors="<<callback->errors<<" signal="<<(callback->signalPresent?1:0)<<" connections="<<connections<<"\n";
    }
    input->StopStreams();input->SetCallback(nullptr);callback->finish();
    if(callback->failed)throw std::runtime_error(callback->failureMessage());
}
void interactiveMenu(){
    for(;;){
        std::cout << "\nSDI -> OMT | DeckLink Duo 2 | 720p50\n"
                     "1  Geraete anzeigen\n2  OMT-Selbsttest (ohne SDI-Capture)\n"
                     "3  SDI-Capture starten\n0  Beenden\nAuswahl: " << std::flush;
        std::string choice;if(!std::getline(std::cin,choice)||choice=="0")return;
        try{
            if(choice=="1")listDevices();
            else if(choice=="2")testOMT();
            else if(choice=="3"){
                listDevices();
                std::cout<<"Der gewaehlte Port muss von der Ausspielsoftware freigegeben sein.\n"
                             "Geraeteindex (0 = Duo (1), leer = abbrechen): "<<std::flush;
                std::string index;if(!std::getline(std::cin,index))return;if(index.empty())continue;
                size_t parsed=0;int device=std::stoi(index,&parsed);
                if(parsed!=index.size())throw std::runtime_error("Ungueltiger Geraeteindex");
                std::cout<<"OMT-Name (Enter = SDI 720p50): "<<std::flush;
                std::string name;if(!std::getline(std::cin,name))return;if(name.empty())name="SDI 720p50";
                running=true;runCapture(device,name,0);
            }else std::cout<<"Bitte 0, 1, 2 oder 3 eingeben.\n";
        }catch(const std::exception& e){std::cerr<<"Fehler: "<<e.what()<<"\n";}
    }
}
int main(int argc,char** argv){std::cout.setf(std::ios::unitbuf);SetConsoleOutputCP(CP_UTF8);SetConsoleCtrlHandler(consoleHandler,TRUE);
    int result=0;
    try{ComScope scope;
        if(argc==1)interactiveMenu();
        else if(argc==2&&std::string(argv[1])=="--list")listDevices();
        else if(argc==2&&std::string(argv[1])=="--status")deviceStatus();
        else if(argc>=2&&std::string(argv[1])=="--selftest")testOMT(argc>=3?argv[2]:"720p50",argc>=4?argv[3]:"Normal",argc>=5?std::stoi(argv[4]):250,argc>=6?argv[5]:"Stereo 1&2");
        else if(argc>=5&&std::string(argv[1])=="--test-source")testOMT(argv[3],argv[4],0,"Stereo 1&2",false,argv[2]);
        else if(argc>=3&&std::string(argv[1])=="--capture"){
            int index=std::stoi(argv[2]);std::string name=argc>=4?argv[3]:"SDI 720p50";int seconds=argc>=5?std::stoi(argv[4]):0;
            if(seconds<0)throw std::runtime_error("Duration must be >= 0");runCapture(index,name,seconds,argc>=6?argv[5]:"720p50",argc>=7?argv[6]:"Normal",argc>=8?argv[7]:"Stereo 1&2",!(argc>=9&&std::string(argv[8])=="--quiet"));
        }else{std::cout<<"SDI to OMT / Windows x64\n--list                       Read-only DeckLink enumeration\n--selftest                   OMT video/audio loopback, no capture\n--capture INDEX [NAME] [SEC] [MODE] [QUALITY] [AUDIO]\nModes: 720p50, 720p60, 1080p50, 1080p60, 1080i50, 1080i60. Quality: High, Normal, Low. Stereo output 48kHz. AUDIO: \"Stereo 1&2\", \"Stereo 3&4\", \"Stereo 5&6\", \"Stereo 7&8\", \"Mono 1\" .. \"Mono 8\".\nPort must be free; SEC=0 runs until Ctrl+C. No connector mapping changes.\n";}
    }catch(const std::exception& e){std::cerr<<e.what()<<"\n";result=1;}
    omt_shutdown();
    if(argc==1&&result!=0){std::cout<<"Enter zum Schliessen..."<<std::flush;std::string line;std::getline(std::cin,line);}
    return result;
}




