#include "mbed.h"

Serial serial(p13, p14);

// The sinewave is created on this pin
AnalogOut aout(p18);

AnalogIn   ain0(A2);
AnalogIn   ain1(A4);

int baudRate = 921600;

char rx_byte = 0;

void handle()
{
  //if(pc.readable()) // <--- this line doesn't matter either way...
  {
    rx_byte = serial.getc();
    serial.putc(rx_byte);  // <--- WITHOUT THIS LINE, THE APPLICATION WILL HANG IF IT RECEIVES WHILE SENDING!
  }

  return;
}

const double pi = 3.141592653589793238462;
const double amplitude = 1.0f;
const double offset = 65535/2;

double ramp(double x)
{
	double _pi_2 = pi / 2;
	double _3_pi_2 = 3 * _pi_2;
	double _2_pi = pi * 2;
	
	x -= pi;
	
	if (x >= 0 && x < _pi_2)
	{
		return -x / _pi_2;
	}
	else if (x >= _pi_2 && x < _3_pi_2)
	{
		return x / _pi_2 - 2;
	}
	else if (x >= _3_pi_2 && x <= _2_pi)
	{
		return -x / _pi_2 + 4;
	}
}

void onePeriod(int sampleCount, double period)
{
    double rads = 0.0;
	uint16_t sample = 0;
	int delay=(int)(((double)1000000*period)/sampleCount)-78;
	
	for (int i = 0; i < sampleCount; i++) {
	rads = (pi * i) / ((float)sampleCount/2);
	sample = (uint16_t)(amplitude * (offset * (sin(rads + pi))) + offset);
	aout.write_u16(sample);
	wait_us(delay); // 278 - 78
	int adc0 = ain0.read_u16();
	int adc1 = ain1.read_u16();
	serial.printf("S%04X%04X",adc0,adc1); // 9 chraracters take 78.125 microseconds with 921600bps
	}
}

int main()
{
	serial.baud(baudRate);
	serial.attach(handle);
    
    while(1)
	{
        // sinewave output
		if (rx_byte == 'G')
		{
			onePeriod(10000,10);
			rx_byte = 0;
		}
		else
		{
			aout.write_u16((uint16_t)(offset));
		}
    }
}