========================================
HitchHiker Sample
========================================

Last change: 8/3/2010

The samples in this VS solution are intended to be used along with the
HitchHiker's Guide white paper. The query examples progress in a lock step
fashion with the discussions in the paper.

This solution contains two console applications that can be set as Startup Projects:

- HelloTollTutorial
- HelloToll

HelloTollTutorial contains HelloTollTutorial.cs - which is a simple
example of end to end programming using the StreamInsight Object Model API.
This example is referred to in the Tutorial section of the HitchHiker
white paper. This program contains two simple queries - one a straight
pass-through query, and the other a query that computes count of events
over a tumbling window of size one minute. This is meant to show you the
scaffolding of a StreamInsight application. The query template binds with
an interval input and an interval output adapter. You can change the binding
to a Point output adapter and experiment with the results.

HelloToll contains HelloToll.cs - which demonstrates an exhaustive set of
StreamInsight query features. When you run the program, it offers you a menu
from you can pick a number for the query you wish to run, and then runs the
query corresponding to your choice. Behind the covers, each of these maps to
a method that binds the relevant LINQ query template to the input and output,
and runs the query. These queries are explained in the HitchHiker white paper,
which can be downloaded at:
http://go.microsoft.com/fwlink/?LinkID=196344&clcid=0x409