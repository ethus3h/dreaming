// Computers and music Workshop
// Folger Library
// 3.27.2017
// by rod o'connor
// https://github.com/sixtycycles

//strings, method, semi-colon.

"Hello World!".postln;

//expression, variable:
(
var output;
output = "Computer music is cool!";
output.postln;
)
//this doesnt work though!
output.postln;

//heres another method call!
// tell the server to start, so we can make sounds!

s.boot;

//now see if everything is working well:

a= Synth(\default);
//to stop it
a.free;

//alternatively you can use "ctrl + ." to stop any sounds in SC.

//some data types:

//string:
"this is a string. it is words and some numbers 2017"
"the important thing to know is that strings are all text, even if there are numbers in the string."
"wow, so cool"
'single quotes are also strings in most languages.'

// integer  or int
1
2
184849275

//float:
0.0
1.0
1.3333333333333333333
12.09834

// list, collection , array:
[]
[1,2,3]
[1,1001,20,"a",Synth()]

//Dict or Dictionary:
{'key1': 1001,'key2':9090,'key3':356}


//CONTROL STRUCTURES:

//logic: if, While, for,do
for( 1 , 10 , {|i| i.postln;} );

//a do loop!
(
var out = 0;
11.do({
	out.postln;
	out=out+1;
});
)

// so a loop is a way to "iterate" over a collection or list. this means it will do some function (action) once for each element in the list.
//in supercollider there are special classes for looping that make music composition much easier! (and less math-y)

// these are called patterns.
(
a = Pseq([1,2,3],2);
b = a.asStream;
)

b.next;



// there are a few special classes that model different kinds of loops.

Pseq = sequentially run through a list of values some number of times.

Prand = randomly select from a list of values some number of times.

Pn = repeat another pattern some number of times.

Pfunc = a function that will return values when asked.

and a special one:

Pbind = for sticking patterns together and binding to an instument.

// these need to be nested to make a proper structure:
(
Pbind(*[
	"instrument" : "Default",
	"degree": Pseq([1,2,3,4,5,6,7,8],1)
]
).play
)



//some stuff for fun:
//these are SynthDefs. technically they are functions that describe synthesis objects on the server.
// more practically, you can think of them as instruments

(
SynthDef("vOsc",{ arg out=0, freq = 440, sustain = 1, amp=1;
	var env = EnvGen.ar(Env.sine, timeScale:sustain, doneAction:2);
	var sig =  SinOsc.ar( freq, 0, amp);
	sig= sig*env;
	sig = Pan2.ar(sig, 0,amp/4);
	OffsetOut.ar(out,sig)
}).store;

SynthDef("vOsc2",{ arg out=0,freq = 440, sustain = 1,amp = 0.3 ;
	var sig =  Saw.ar( freq, 1);
	var env = EnvGen.ar(Env.perc,timeScale:sustain, doneAction:2);
	sig = Pan2.ar(RLPF.ar(sig*env,freq*3 ,1.5,1)*env, 0,amp);
	OffsetOut.ar(out,sig)
}).store;
)



(
//here we wil set a variable called a to the Scale Object.
//effectively this tells the environment that we're playing in this key and tuning.

//if you run Sca le.directory; you can see a list of the available scales.
Scale.directory;

a = Scale.chromatic;

//and here are some patterns that  bind phrases to  the instruments.
//fthe first line inside the Pbind tells us what instrument we are playing.
//degree represents a scale degree. think of this as notes in a scale, but with numbers instead of letters.(degree 1 = c, degree 2 = c# ect....

Pbind(
	\instrument, \vOsc2,
	\degree, Pseq([-12,-24,-12,-24,-12,-24,-14,-26,-16,-28,-16,-28,-16,-28,-14,-26],inf),
	\scale,Pfunc({a},inf),
	\dur, Pseq([0.6],inf),
	\amp,Pseq([0.4,2],inf)
) .play();

Pbind(
	\instrument, \vOsc2,
	\degree, Pseq([
		Pseq([7,7,5,3, 5,7,0,-2, 7,7,5,3, 5,7,5],1),
		Pseq([7,5,3, 5,7,0, 7,5,3, 5,7,3,2],1),
	],inf),
	\scale,Pfunc({a},inf),
	\dur, Pseq([
		Pseq([0.3],14),
		Pseq([0.6],2),
		Pseq([0.3],4),
		Pseq([0.6],2),
		Pseq([0.3],5),
	],inf),
	\legato, Pseq([4],inf),
	\amp,Pseq([0.1],inf)
).play();
)


//here is an example of a different type of synthesis.
//this is called granular synthesis, and uses a sound file for input data, rather than an oscilator!


(
SynthDef(\timeMachine, { | out=0, bufnum = 0, rate = 1, startPos = 0, amp = 0.1, sustain = 1, pan = 0, loop = 1|
    var audio;
    rate = rate * BufRateScale.kr(bufnum);
    startPos = startPos * BufFrames.kr(bufnum);

    audio = BufRd.ar(1, bufnum, Phasor.ar(0, rate, startPos, BufFrames.ir(bufnum)), 1, 1);
    audio = EnvGen.ar(Env.sine, 1, timeScale: sustain, doneAction: 2) * audio;
    audio = Pan2.ar(audio, pan, amp);
    OffsetOut.ar(out, audio);
}).add;

a = Pproto({
    ~newgroup = (type: \group).yield;
    ~sf1 = (type: \allocRead, path: Platform.resourceDir +/+ "sounds/a11wlk01-44_1.aiff").yield;
},
    Pdef(\a,Pbind(*[
        instrument:    \timeMachine,
	        dur:        Pseg([0,0,0.25,0.5, 0.75, 1],10,'exp'),
        legato:        4,
        startPos:    Pn(Pseg([0,1], 10), inf),
	    rate:        [1,1.333,1.567],
        loop:        0,
        group:        Pkey(\newgroup),
        bufnum:        Pkey(\sf1)
    ])
);
);
a.play;
)

