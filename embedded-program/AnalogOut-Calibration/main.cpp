#include "mbed.h"

Serial serial(p13, p14);

// The sinewave is created on this pin
AnalogOut aout(p18);

AnalogIn   ain0(A2);
AnalogIn   ain1(A4);

int baudRate = 460800;

const double pi = 3.141592653589793238462;
const double amplitude = 1.0f;
const double offset = 65535/2;

double triangle(double x)
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

double halfTriangle(double x)
{
	double _pi_2 = pi / 2;
	double _2_pi = pi * 2;
	
	x -= pi;
	
	if (x >= 0 && x < pi)
	{
		return -x / pi;
	}
	else if (x >= pi && x < _2_pi)
	{
		return x / pi - 2;
	}
}

double halfSine(double x)
{
	return (sin((x+pi)/2));
}

void onePeriod(void)
{
    double rads = 0.0;
	uint16_t sample = 0;
	
	for (int i = 0; i < 1000; i++) {
	rads = (pi * i) / 500.0f;
	sample = (uint16_t)(amplitude * (offset * (-1.0f/*halfSine(rads + pi)*/)) + offset);
		//wait_ms(1);
	aout.write_u16(sample);
	}
}

int main()
{
	//serial.puts("Hello World!");
    while(1) {
        // sinewave output
		onePeriod();

    }
}