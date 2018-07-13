#include "stdafx.h"
#include "MPS.h"
#include <thread>
#include <chrono>
#include <vector>
#include <numeric>
#include <iostream>
#include <ctime>

using namespace std::chrono;

//Determining platform's tic period
const double microsPerClkTic{
	1.0E6 * system_clock::period::num / system_clock::period::den
};

//Processing period 
const milliseconds intervalPeriodMillis{ 1000 };

MPS::MPS()
{
}


MPS::~MPS()
{
}

void MPS::getMPS()
{
	//Let's define the start time and the next time the thread will wake up
	system_clock::time_point currentStartTime{ system_clock::now() };
	system_clock::time_point nextStartTime{ currentStartTime };

	while (true)
	{
		//getting our "wakeup" time
		currentStartTime = system_clock::now();

		//determing the next time the thread will get executed
		nextStartTime = currentStartTime + intervalPeriodMillis;
		// TODO: code
		std::cout << "ciao";
		
		//Sleep till our next period start time
		std::this_thread::sleep_until(nextStartTime);
	}

}
