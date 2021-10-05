#include "mbed.h"

Serial serial(p13, p14);
Serial pc(USBTX, USBRX);

// The sinewave is created on this pin
AnalogOut aout(p18);

AnalogIn   ain0(A2);
AnalogIn   ain1(A4);
uint16_t map;
int baudRate = 921600;

char getHexValue(char chr)
{
	char n = 16;

	switch (chr)
	{
		case 'F':
			n = 15;
			break;
		case 'E':
			n = 14;
			break;
		case 'D':
			n = 13;
			break;
		case 'C':
			n = 12;
			break;
		case 'B':
			n = 11;
			break;
		case 'A':
			n = 10;
			break;
		case '9':
			n = 9;
			break;
		case '8':
			n = 8;
			break;
		case '7':
			n = 7;
			break;
		case '6':
			n = 6;
			break;
		case '5':
			n = 5;
			break;
		case '4':
			n = 4;
			break;
		case '3':
			n = 3;
			break;
		case '2':
			n = 2;
			break;
		case '1':
			n = 1;
			break;
		case '0':
			n = 0;
			break;
	}

	return n;
}

char rx_byte = 0;
char rx_buffer[16];
unsigned char rx_count = 0;
bool cmdProcessing = false;
bool cmdReceived = false;
bool signalGenerating = false;
bool resistance = false;
bool firstTime = true;

char globalType = 0;  // 0 : sin | 1 : ramp
float globalAmplitude = 1.0f;  // 1.0f : 2.5v
uint16_t golbalRate = 100;
double globalPeriod = 1.0f;
signed char globalSign = 1;

void rx_interrupt()
{
	LPC_UART0->IER = 0;         // Temporary Disable RX Interrupt
	
    rx_byte = LPC_UART1->RBR;   // Read Receiver Buffer Register
	
	if (!cmdProcessing)
	{
		if (rx_byte == 'Q')
		{
			cmdProcessing = true;
		}
		
		if (rx_byte == 'G')
		{signalGenerating = true;resistance = false;firstTime = true;}
		
		if (rx_byte == 'P')
		{signalGenerating = false;resistance = false;}
		
		if (rx_byte == 'R')
			resistance = true;
	}
	else
	{
		rx_buffer[rx_count] = rx_byte;
		rx_count++;
		
		if (rx_count == 12)
		{
			cmdReceived = true;
			cmdProcessing = false;
			rx_count = 0;		
		}
	}
	
	LPC_UART0->IER = 1;			// Re-enable RX Interrupt
}

const double pi = 3.141592653589793238462;
const double _pi_2 = pi / 2;
const double _3_pi_2 = 3 * _pi_2;
const double _2_pi = pi * 2;

const double offset = 65535/2;
const uint16_t sampleRate = 100;

double triangle(double x)
{	
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
	
	return 0;
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

double signalOut(char type, double x)
{
	switch (globalType)
	{
		case 0:
			return sin(x);
		case 1:
			return triangle(x);
		case 2:
			return halfSine(x);
		case 3:
			return halfTriangle(x);
		case 4:
			return -1.0;
		default:
			return 0;
	}
}

void onePeriod(char type, signed char sign, float amplitude, double period)
{
    double rads = 0.0;
	uint16_t sample = 0;
	
	if (period > 100.0) period = 100.0;
	else if (period < 1.0) period = 1.0;
	
	if (amplitude > 1.0f) amplitude = 1.0f;
	else if (amplitude < 0.0f) amplitude = 0.0f;
	
	// SampleRate = 100
	int delay = 9910; // (1000000 / SampleRate) - 78:serial + 2x5:adc + 2:dac
	
	uint32_t sampleCount = (uint32_t)(sampleRate * period);
	
	uint16_t adc0;
	uint16_t adc1;
	
	for (uint32_t i = 0; i < sampleCount; i++)
	{
		if (signalGenerating && !resistance)
		{
			rads = (pi * i) / ((float)sampleCount / 2);
				
			sample = (uint16_t)(amplitude * (offset * (sign * signalOut(type, rads + pi))) + offset);
			aout.write_u16(sample);
				
			wait_us(delay);
				
			adc0 = ain0.read_u16();
			adc1 = ain1.read_u16();
			
			if (adc0 > 65530 || adc0 < 5  || adc1 > 65530 || adc1 < 5)  // Over-Current or Over-Voltage
			{
				amplitude = 0;
				globalAmplitude = 0;
				signalGenerating = false;
				serial.putc('W');
			}
			else
			{	
				serial.printf("S%04X%04X", adc0, adc1); // 9 chraracters take 78.125 microseconds with 921600bps
			}
		}
		else
		{			
			break;
		}
	}
}

int main()
{
	serial.baud(baudRate);
	serial.attach(&rx_interrupt, Serial::RxIrq);
    
    while(1)
	{	
		uint16_t adc0;
		uint16_t adc1;	
		
		uint32_t adc0_sum;
		uint32_t adc1_sum;
		
		if (signalGenerating  && !resistance)
		{			
			aout.write_u16((uint16_t)(globalAmplitude * (offset * (-1)) + offset));
			
			if (firstTime)
			{
				wait_ms(10);
				firstTime = false;
			}
			
			adc0_sum = 0;
			adc1_sum = 0;
			
			for(char i = 0; i < 100; i++)
			{
				wait_us(99);
				
				adc0 = ain0.read_u16();
				adc1 = ain1.read_u16();
				
				adc0_sum += adc0;
				adc1_sum += adc1;
			}
			
			adc0 = (uint16_t)((double)adc0_sum / 100.0);
			adc1 = (uint16_t)((double)adc1_sum / 100.0);
			
			if (adc0 > 65530 || adc0 < 5  || adc1 > 65530 || adc1 < 5)  // Over-Current or Over-Voltage
			{
				globalAmplitude = 0;
				signalGenerating = false;
				serial.putc('W');
			}
			else
			{	
				serial.printf("S%04X%04X", adc0, adc1); // 9 chraracters take 78.125 microseconds with 921600bps
			}		
		}
		else
		{
			if (resistance)
			{
				aout.write_u16((uint16_t)(offset));
				
				wait_ms(998);
				
				aout.write_u16((uint16_t)(globalAmplitude * (offset * (-1)) + offset));
					
				adc0_sum = 0;
				adc1_sum = 0;
				
				wait_ms(1);
				
				for(char i = 0; i < 100; i++)
				{	
					wait_us(10);
					
					adc0 = ain0.read_u16();
					adc1 = ain1.read_u16();
					
					adc0_sum += adc0;
					adc1_sum += adc1;
				}
				
				adc0 = (uint16_t)((double)adc0_sum / 100.0);
				adc1 = (uint16_t)((double)adc1_sum / 100.0);
				
				if (adc0 > 65530 || adc0 < 5  || adc1 > 65530 || adc1 < 5)  // Over-Current or Over-Voltage
				{
					globalAmplitude = 0;
					signalGenerating = false;
					serial.putc('W');
				}
				else
				{	
					serial.printf("S%04X%04X", adc0, adc1); // 9 chraracters take 78.125 microseconds with 921600bps
				}
			}
			else
			{
				aout.write_u16((uint16_t)(offset));
			
			if (cmdReceived)
			{
				char d1 = getHexValue(rx_buffer[0]);				
				char d2 = getHexValue(rx_buffer[1]);
				
				char d3[2];
				
				d3[1] = getHexValue(rx_buffer[2]);
				d3[0] = getHexValue(rx_buffer[3]);
				
				char d4[8];
				
				d4[7] = getHexValue(rx_buffer[4]);
				d4[6] = getHexValue(rx_buffer[5]);
				d4[5] = getHexValue(rx_buffer[6]);
				d4[4] = getHexValue(rx_buffer[7]);
				d4[3] = getHexValue(rx_buffer[8]);
				d4[2] = getHexValue(rx_buffer[9]);
				d4[1] = getHexValue(rx_buffer[10]);
				d4[0] = getHexValue(rx_buffer[11]);

				if (!( d1 == 16 || d2 == 16 || d3[1] == 16 || d3[0] == 16 || d4[7] == 16 || d4[6] == 16 || d4[5] == 16 || d4[4] == 16 || d4[3] == 16 || d4[2] == 16 || d4[1] == 16 || d4[0] == 16 ))
				{
					globalType = d1;
					if (d2 == 15) globalSign = 1;
					else if (d2 == 0) globalSign = -1;
					globalAmplitude = (float)(d3[1] * 16 + d3[0]) / 255.0f;
					globalPeriod = (double)(d4[7] * 268435456 + d4[6] * 16777216 + d4[5] * 1048576 + d4[4] * 65536 + d4[3] * 4096 + d4[2] * 256 + d4[1] * 16 + d4[0]) / 42949672.0;			
				}	
				
				cmdReceived = false;
			}
		}
		}
    }
}
