% t25 = csvread('2.5.txt',1,0,[1,0,NaN,0]);
% v25 = csvread('2.5.txt',1,1,[1,1,NaN,1]);
% i25 = csvread('2.5.txt',1,2,[1,2,NaN,2]);
% 
% tr25 = csvread('2.5r.txt',1,0,[1,0,NaN,0]);
% vr25 = csvread('2.5r.txt',1,1,[1,1,NaN,1]);
% ir25 = csvread('2.5r.txt',1,2,[1,2,NaN,2]);

%% 1.0v Load Data

data1 = load('1.0.mat');

t1 = data1.t1;
v1 = data1.v1;
i1 = data1.i1;

tr1 = data1.tr1;
vr1 = data1.vr1;
ir1 = data1.ir1;

v1=v1-i1*1494;
vr1=vr1-ir1*1494;

r1=v1./i1;
rr1=vr1./ir1;

%% 1.0v Plot Data

figure(1);

subplot(1,3,1);
plot(t1,v1,'b');
xlabel('Time (seconds)')
ylabel('Voltage (volts)')
hold on;
plot(tr1,vr1,'r');
grid on;

subplot(1,3,2);
plot(t1,i1,'b');
xlabel('Time (seconds)')
ylabel('Current (volts)')
hold on;
plot(tr1,ir1,'r');
grid on;

subplot(1,3,3);
plot(t1,r1,'b');
xlabel('Time (seconds)')
ylabel('Resistance (ohms)')
hold on;
plot(tr1,rr1,'r');
grid on;

%% 1.5v Load Data

data15 = load('1.5.mat');

t15 = data15.t15;
v15 = data15.v15;
i15 = data15.i15;

tr15 = data15.tr15;
vr15 = data15.vr15;
ir15 = data15.ir15;

v15=v15-i15*1494;
vr15=vr15-ir15*1494;

r15=v15./i15;
rr15=vr15./ir15;

%% 1.5v Plot Data

figure(15);

subplot(1,3,1);
plot(t15,v15,'b');
xlabel('Time (seconds)')
ylabel('Voltage (volts)')
hold on;
plot(tr15,vr15,'r');
grid on;

subplot(1,3,2);
plot(t15,i15,'b');
xlabel('Time (seconds)')
ylabel('Current (volts)')
hold on;
plot(tr15,ir15,'r');
grid on;

subplot(1,3,3);
plot(t15,r15,'b');
xlabel('Time (seconds)')
ylabel('Resistance (ohms)')
hold on;
plot(tr15,rr15,'r');
grid on;

%% 2.0v Load Data

data2 = load('2.0.mat');

t2 = data2.t2;
v2 = data2.v2;
i2 = data2.i2;

tr2 = data2.tr2;
vr2 = data2.vr2;
ir2 = data2.ir2;

v2=v2-i2*1494;
vr2=vr2-ir2*1494;

r2=v2./i2;
rr2=vr2./ir2;

%% 2.0v Plot Data

figure(2);

subplot(1,3,1);
plot(t2,v2,'b');
xlabel('Time (seconds)')
ylabel('Voltage (volts)')
hold on;
plot(tr2,vr2,'r');
grid on;

subplot(1,3,2);
plot(t2,i2,'b');
xlabel('Time (seconds)')
ylabel('Current (volts)')
hold on;
plot(tr2,ir2,'r');
grid on;

subplot(1,3,3);
plot(t2,r2,'b');
xlabel('Time (seconds)')
ylabel('Resistance (ohms)')
hold on;
plot(tr2,rr2,'r');
grid on;
%% 2.5v Load Data

data25 = load('2.5.mat');

t25 = data25.t25;
v25 = data25.v25;
i25 = data25.i25;

tr25 = data25.tr25;
vr25 = data25.vr25;
ir25 = data25.ir25;

v25=v25-i25*1494;
vr25=vr25-ir25*1494;

r25=v25./i25;
rr25=vr25./ir25;

%% 2.5v Plot Data

figure(25);

subplot(1,3,1);
plot(t25,v25,'b');
xlabel('Time (seconds)')
ylabel('Voltage (volts)')
hold on;
plot(tr25,vr25,'r');
grid on;

subplot(1,3,2);
plot(t25,i25,'b');
xlabel('Time (seconds)')
ylabel('Current (volts)')
hold on;
plot(tr25,ir25,'r');
grid on;

subplot(1,3,3);
plot(t25,r25,'b');
xlabel('Time (seconds)')
ylabel('Resistance (ohms)')
hold on;
plot(tr25,rr25,'r');
grid on;
