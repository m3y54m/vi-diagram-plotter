#include "mbed.h"

Serial serial(p13, p14);

// The Output analog signal is created on this pin
AnalogOut aout(p18);

AnalogIn   ain0(A2);
AnalogIn   ain1(A4);

float vref = 2.495f;

int baudRate = 921600;

char rx_byte = 0;
char rx_buffer[32];
unsigned char rx_count = 0;
bool cmdProcessing = false;
bool cmdReceived = false;
bool signalGenerating = false;

char globalType = 0;
float globalAmplitude = 1.0f;  // 0 ~ 1.0
float globalPeriod = 1.0f;
uint32_t globalFrequency = 1000;
uint16_t golbalRate = 1000;
signed char globalSign = 1;
bool IsHighFreq = false;


void rx_interrupt()
{
	LPC_UART0->IER = 0;         // Temporary Disable RX Interrupt (IMPORTANT)
	
  rx_byte = LPC_UART1->RBR;   // Read Receiver Buffer Register
	
	if (!cmdProcessing)
	{
		if (rx_byte == 'Q')
		{
			cmdProcessing = true;
		}
		
		if (rx_byte == 'G')
			signalGenerating = true;
		
		if (rx_byte == 'P')
			signalGenerating = false;
	}
	else
	{
		rx_buffer[rx_count] = rx_byte;
		rx_count++;
		
		if (rx_count == 32) // recevied command bytes count except 'Q'
		{
			cmdReceived = true;
			cmdProcessing = false;
			rx_count = 0;		
		}
	}
	
	LPC_UART0->IER = 1;			// Re-enable RX Interrupt
}

char getHexValue(char chr);

const double pi = 3.141593;
const double offset = 512;
const double half_scale = 511;
const uint16_t sampleRate = 1000;

double triangle(double x);
double halfTriangle(double x);
double halfSine(double x);
double signalOut(char type, double x);
void onePeriod(char type, float amplitude, float period);
void Periodic(void);

uint32_t counter;

void writeVoltage(uint16_t dac)
{
	aout.write_u16(dac << 6);
}

uint16_t readVoltage(void)
{
	return ain0.read_u16() >> 4;
}

uint16_t readCurrent(void)
{
	return ain1.read_u16() >> 4;
}

uint16_t adc0;
uint16_t adc1;

int main()
{
	serial.baud(baudRate);
	serial.attach(&rx_interrupt, Serial::RxIrq);
	
//				char str[50];
//				sprintf(str,"%d,%d\r\n",adc0,adc1);
//				serial.puts(str);
	
  while(1)
	{
		
		if (signalGenerating)
		{			
			if (IsHighFreq)
			{
				Periodic();	
			}
			else
			{
				onePeriod(globalType, globalAmplitude, globalPeriod);			
				signalGenerating = false;
			}
		}
		else
		{
			writeVoltage((uint16_t)(offset));
			
			if (cmdReceived)
			{
				uint32_t tempLong;
				char hexDigits[32];
				bool validData = true;
				unsigned char configByte;
				
				for (char i = 0; i < 32; i++)
				{
					hexDigits[i] = getHexValue(rx_buffer[i]);
					
					if (hexDigits[i] == 16)
					{
						validData = false;
						break;
					}
				}
				
				if (validData)
				{
					globalType = (hexDigits[0] << 4) | hexDigits[1];
					
					tempLong = ((uint32_t)((hexDigits[2] << 4) | hexDigits[3]) << 24) | ((uint32_t)((hexDigits[4] << 4) | hexDigits[5]) << 16) | ((uint32_t)((hexDigits[6] << 4) | hexDigits[7]) << 8) | (uint32_t)((hexDigits[8] << 4) | hexDigits[9]);
					globalAmplitude = *((float*)(&tempLong));
					
					tempLong = ((uint32_t)((hexDigits[10] << 4) | hexDigits[11]) << 24) | ((uint32_t)((hexDigits[12] << 4) | hexDigits[13]) << 16) | ((uint32_t)((hexDigits[14] << 4) | hexDigits[15]) << 8) | (uint32_t)((hexDigits[16] << 4) | hexDigits[17]);
					globalPeriod = *((float*)(&tempLong));
					
					globalFrequency = ((uint32_t)((hexDigits[18] << 4) | hexDigits[19]) << 24) | ((uint32_t)((hexDigits[20] << 4) | hexDigits[21]) << 16) | ((uint32_t)((hexDigits[22] << 4) | hexDigits[23]) << 8) | (uint32_t)((hexDigits[24] << 4) | hexDigits[25]);
					
					golbalRate = ((uint16_t)((hexDigits[26] << 4) | hexDigits[27]) << 8) | (uint16_t)((hexDigits[28] << 4) | hexDigits[29]);
					
					configByte = (hexDigits[30] << 4) | hexDigits[31];
					
					globalSign = (configByte & 0x80) >> 7;
					
					if (globalSign == 0)
						globalSign = -1;
					
					configByte = (configByte & 0x40) >> 6; // IsHighFreq
					
					if (configByte == 1)
						IsHighFreq = true;
					else
						IsHighFreq = false;
				}
				
				counter = 0;
				
				cmdReceived = false;
			}
		}
  }
}

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
			return -sin(x);
		case 1:
			return -triangle(x);
		case 2:
			return -halfSine(x);
		case 3:
			return -halfTriangle(x);
		case 4:
			return 1.0;
		default:
			return 0;
	}
}

void onePeriod(char type, float amplitude, float period)
{
  double rads = 0.0;
	uint16_t sample = 0;
	
	if (period > 100.0f) period = 100.0f;
	else if (period < 0.1f) period = 0.1f;
	
	if (amplitude > 1.0f) amplitude = 1.0f;
	else if (amplitude < 0.0f) amplitude = 0.0f;
	
	// SampleRate = 1000
	int delay = 880; // (100000 / SampleRate) - 78:serial + 2x19:adc + 4:dac
	
	uint32_t sampleCount = (uint32_t)(sampleRate * period);
	
	for (uint32_t i = 0; i < sampleCount; i++)
	{
		if (signalGenerating)
		{
			rads = (pi * i) / ((float)sampleCount / 2);
	
			sample = (uint16_t)(amplitude * (half_scale * (globalSign*signalOut(type, rads + pi))) + offset);

			writeVoltage(sample);  //4us
	
			wait_us(delay);
				
			adc0 = readVoltage(); //19us
			adc1 = readCurrent(); //19us
			
			if (adc0 == 4095 || adc0 == 0  || adc1 == 4095 || adc1 == 0)  // Over-Current or Over-Voltage
			{
				amplitude = 0;
				globalAmplitude = 0;
				signalGenerating = false;
				serial.putc('W'); // error condition
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
	
	if (signalGenerating)
		serial.putc('P');  // end of period
}

void Periodic(void)
{
	uint16_t sample = 0;
	
	if (globalFrequency == 0)
	{
			sample = (uint16_t)(globalAmplitude * (half_scale * (globalSign*(1))) + offset); // DC
			writeVoltage(sample);
		
			wait_us(8);
				
			adc0 = readVoltage(); //19us
			adc1 = readCurrent(); //19us
			
			if (adc0 == 4095 || adc0 == 0  || adc1 == 4095 || adc1 == 0)  // Over-Current or Over-Voltage
			{
				globalAmplitude = 0;
				signalGenerating = false;
				serial.putc('W'); // error condition
			}
			
			counter++;
			
			if (counter == 5000)
			{	
				counter = 0;
				if (signalGenerating)
				{	
						serial.printf("S%04X%04X", adc0, adc1); // 9 chraracters take 78.125 microseconds with 921600bps
				}
			}	
	}
	else
	{
		uint16_t period_s_2;  // one half of period in seconds
		uint16_t period_ms_2; // one half of period in milliseconds
		uint32_t period_us_2; // one half of period in microseconds
		
		uint32_t freq = globalFrequency;
		
		int16_t first_us = 0;
		
		if ( freq > 5000) freq = 5000;
		
		period_us_2 = (uint32_t)(500000.0f / (float)freq);
		
		period_ms_2 = (uint16_t)(period_us_2 / 1000);
		
		period_us_2 = period_us_2 % 1000;
		
		period_s_2 = period_ms_2 / 1000;
		
		period_ms_2 = period_ms_2 % 1000;
		
		sample = (uint16_t)(globalAmplitude * (half_scale * (globalSign*(1))) + offset); // DC
		writeVoltage(sample); //4us
		
		adc0 = readVoltage(); //19us
		adc1 = readCurrent(); //19us
		
		if (adc0 == 4095 || adc0 == 0  || adc1 == 4095 || adc1 == 0)  // Over-Current or Over-Voltage
		{
			globalAmplitude = 0;
			signalGenerating = false;
			serial.putc('W'); // error condition
		}
		
		wait(period_s_2);
		wait_ms(period_ms_2);
		
		first_us = period_us_2 - 84;
		
		if (first_us > 0) wait_us(first_us);
			
		adc0 = readVoltage(); //19us
		adc1 = readCurrent(); //19us
		
		writeVoltage(offset); //4us
		
		counter++;
		
		if (freq > 10) freq = freq / 10;
		
		if (counter == freq)
		{	
			counter = 0;
			if (signalGenerating)
			{	
					serial.printf("S%04X%04X", adc0, adc1); // 9 chraracters take 78.125 microseconds with 921600bps
			}
		}	
		
		wait(period_s_2);
		wait_ms(period_ms_2);
		wait_us(period_us_2);
	}

}