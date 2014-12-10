sox orig.wav low.wav sinc -150 -t 10
sox low.wav low_eq.wav gain -n
sox orig.wav high.wav sinc 150 -t 10
sox high.wav high_eq.wav gain -n
sox -m low_eq.wav high_eq.wav sox.wav