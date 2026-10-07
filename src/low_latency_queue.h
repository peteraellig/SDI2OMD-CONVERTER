#pragma once
#include <chrono>
#include <condition_variable>
#include <cstdint>
#include <deque>
#include <mutex>
#include <optional>
#include <utility>

// Capture never waits for encoding. Audio is bounded by duration, not packet size.
// Kept independent of DeckLink/OMT so overload and recovery can be tested exactly.
template<class Packet> class LowLatencyQueue {
public:
    using Clock = std::chrono::steady_clock;
    struct Stats { uint64_t videoDropped=0, audioSamplesDropped=0; size_t audioPackets=0;
        int audioMs=0, videoMs=0; };
private:
    struct Entry { Packet packet; Clock::time_point arrived; int64_t timestamp; int samples; };
    std::mutex mutex;
    std::condition_variable wake;
    std::optional<Entry> video;
    std::deque<Entry> audio;
    int samples=0;
    bool closed=false;
    const std::chrono::microseconds maxVideoAge;
    static constexpr int maxAudioSamples=5760; // 120 ms, stereo 48 kHz.
    uint64_t lostVideo=0, lostAudio=0;
    void dropAudio(){ lostAudio+=audio.front().samples; samples-=audio.front().samples; audio.pop_front(); }
    void prune(Clock::time_point now){
        if(video && now-video->arrived>maxVideoAge){++lostVideo;video.reset();}
        while(!audio.empty() && now-audio.front().arrived>std::chrono::milliseconds(120))dropAudio();
    }
public:
    explicit LowLatencyQueue(std::chrono::microseconds videoAge):maxVideoAge(videoAge){}
    void push(Packet&& packet,bool isVideo,int64_t timestamp,int sampleCount=0,Clock::time_point now=Clock::now()){
        std::lock_guard lock(mutex); if(closed)return; prune(now);
        if(isVideo){if(video)++lostVideo;video=Entry{std::move(packet),now,timestamp,0};}
        else {
            if(sampleCount<=0)return;
            if(sampleCount>maxAudioSamples){lostAudio+=sampleCount;return;}
            while(!audio.empty() && (samples+sampleCount>maxAudioSamples || audio.size()>=64))dropAudio();
            samples+=sampleCount;audio.push_back(Entry{std::move(packet),now,timestamp,sampleCount});
        }
        wake.notify_one();
    }
    std::optional<Packet> take(Clock::time_point now=Clock::now()){
        std::lock_guard lock(mutex); prune(now); if(closed)return {};
        // Send the older audio first. Under overload this drains the preserved
        // audio before encoding the newest image, without rewriting timestamps.
        if(!audio.empty() && (!video || audio.front().timestamp<=video->timestamp)){
            Packet result=std::move(audio.front().packet);samples-=audio.front().samples;audio.pop_front();return result;
        }
        if(video){Packet result=std::move(video->packet);video.reset();return result;}
        return {};
    }
    std::optional<Packet> wait(){
        for(;;){
            {std::unique_lock lock(mutex);wake.wait(lock,[this]{return closed||video.has_value()||!audio.empty();});if(closed)return {};}
            if(auto packet=take())return packet;
        }
    }
    void close(){std::lock_guard lock(mutex);closed=true;video.reset();audio.clear();samples=0;wake.notify_all();}
    void discardAudioSamples(int count){std::lock_guard lock(mutex);lostAudio+=count;}
    void clearVideo(){std::lock_guard lock(mutex);video.reset();}
    Stats stats(Clock::time_point now=Clock::now()){
        std::lock_guard lock(mutex);
        return {lostVideo,lostAudio,audio.size(),samples*1000/48000,
            video ? static_cast<int>(std::chrono::duration_cast<std::chrono::milliseconds>(now-video->arrived).count()) : 0};
    }
};
