========================================
PatternDetector Sample
========================================

Last change: 8/26/2010

This sample uses the StreamInsight extensibility framework to implement a
pattern detection application. The pattern matching user-defined operator is
located in AugmentedFiniteAutomaton, and is based on an augmented finite automaton
(AFA) - which is basically a non-deterministic finite automaton (NFA) with
additional information, called a register, that can be created and maintained
as part of the automaton during runtime.

The example uses a sample stock quote stream as an input of point events, 
and defines an AFA that detects a sequence of downticks followed by an 
equal number of upticks. We detect the pattern over a tumbling window of 
one hour. The corresponding AFA is shown in the file AFAexample.pptx, 
and is implemented in AFA_EqualDownticksUpticks.cs.

Note that pattern detection operates directly over a point event stream, 
and currently does not work with snapshot windows with AlterEventDuration
(i.e., sliding windows). The reason is that StreamInsight currently 
supports a single input policy of clipping both the start time and end 
time of events in a window to the window start time and end time. This 
means that any time-sensitive set-based operation (such as this pattern 
detection UDO) will only see the original start times of events that 
begin inside the window.

For more information on AFA, see:

Badrish Chandramouli, Jonathan Goldstein, and David Maier. High-Performance 
Dynamic Pattern Matching over Disordered Streams. In VLDB 2010.
