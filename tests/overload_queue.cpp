#include "low_latency_queue.h"
#include <future>
#include <iostream>
#include <stdexcept>
struct Packet { int id; };
using Queue=LowLatencyQueue<Packet>;
using namespace std::chrono_literals;
void require(bool value,const char* message){if(!value)throw std::runtime_error(message);}
int main(){
    try {
        const auto origin=Queue::Clock::time_point{};
        Queue queue(40ms);
        for(int i=0;i<5;++i){auto time=origin+i*20ms;queue.push(Packet{i},true,i*200000,0,time);queue.push(Packet{100+i},false,i*200000,960,time);}
        require(queue.stats(origin+80ms).videoDropped==4,"Oldest waiting video must be replaced");
        for(int i=0;i<5;++i){auto packet=queue.take(origin+80ms);require(packet && packet->id==100+i,"Audio must survive a short encoder stall, in order");}
        require(queue.take(origin+80ms)->id==4,"Most recent video must be encoded");
        require(!queue.take(origin+80ms),"No stale backlog allowed");
        Queue stalled(40ms);
        for(int i=0;i<10;++i){auto time=origin+i*20ms;stalled.push(Packet{i},true,i*200000,0,time);stalled.push(Packet{100+i},false,i*200000,960,time);}
        auto state=stalled.stats(origin+180ms);
        require(state.audioMs==120 && state.audioSamplesDropped==3840,"Audio backlog must be capped at 120 ms");
        require(stalled.take(origin+180ms)->id==104,"Discard oldest audio, preserve freshest audio");
        require(!stalled.take(origin+400ms),"Long stall must discard expired audio and video");
        auto expired=stalled.stats(origin+400ms);
        require(expired.videoDropped==10 && expired.audioSamplesDropped==8640,"Expired packet accounting failed");
        Queue bounded(40ms);
        for(int i=0;i<10000;++i)bounded.push(Packet{i},false,i,1,origin);
        require(bounded.stats(origin).audioPackets==64,"Tiny audio packets must not grow memory without limit");
        Queue closing(40ms);closing.push(Packet{1},true,0,0,origin);closing.close();
        require(!closing.take(origin),"Stop must discard queued packets");
        Queue blocked(40ms);
        auto waiter=std::async(std::launch::async,[&]{return blocked.wait();});blocked.close();
        require(waiter.wait_for(1s)==std::future_status::ready && !waiter.get(),"Stop must wake a waiting worker");
        std::cout<<"PASS: fresh video, continuous short-stall audio, bounded backlog, expiry and stop\n";
        return 0;
    }catch(const std::exception& e){std::cerr<<e.what()<<'\n';return 1;}
}
