========================================
DataGenerator Input Adapter
========================================

Last change: 5/24/2010

The data generator input adapters simulate an asynchronous data source, that
has the capability to push events into a callback function. Point, Interval,
and Edge flavors are provided. The data source uses a timer to trigger the
generation of a data item with values that are random, but bound by the adapter
configuration.
Multiple event-generating devices can be simulated though the config information,
which all publish through the same single input adapter into a single stream.
Each device is identified by the DeviceID field. Each time the timer triggers,
one single simulated device generates a data item.
The edge version of this adapter sends an end edge for each device as soon as
a new value for that device is pushed from the data source. Since end edges
are matched to start edges by the payloas and start time, this information needs
to be cached in the for each logical device.

These adapter demonstrate the following aspects:

  * How to interface with an asynchronous data source in a true push-model.
  * How to handle the adapter state diagram in the asynchronous case.
  * How to properly handle edge events for multiple logical streams in the
    same physical stream (here: multiple devices)
