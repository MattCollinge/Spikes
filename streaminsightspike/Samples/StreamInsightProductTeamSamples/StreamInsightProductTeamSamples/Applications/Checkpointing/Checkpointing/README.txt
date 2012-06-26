========================================
Checkpointing Sample
========================================

Last change: 15 June 2011

This sample demonstrates the new checkpointing capabilities in StreamInsight
1.2. There are four major artifacts in this application:

	1) A replayable CSV input adapter (ReplayablePointCsvInputAdapter).

	   This input adapter reads comma-separated value (CSV) files of the form

	      DateTimeOffset,double,double

	   Where the DateTimeOffset value is interpreted as the application-time
	   timestamp for the row, and the two doubles are interpreted as X and Y
	   point values.

	   This input adapter can be created with a high-water mark to replay from,
	   and will then scan the file and replay only those values that occur
	   after this value. Replay is coordinated by the associated factory.

	2) A deduplicating CSV output adapter (DedupingPointCsvOutputAdapter).

	   This output adapter writes comma-separted value (CSV) files of the form

	      DateTimeOffset,field1,field2...

	   Where the DateTimeOffset value is the application time of the output
	   event, and the fields are the fields from the event payload.

	   This output adapter can be created with a high-water-mark--event-offset
	   pair that indicates where to begin deduplication. The adapter will
	   memorize values after this point, and will remove duplicates that it
	   encounters as it encounters them.

	3) A class to manage a StreamInsight application (AppManager).

	   This class maintains an application and performs housekeeping on its
	   associated queries. This housekeeping includes taking checkpoints in a
	   round-robin fashion for all of its resilient queries, and removing
	   queries as they complete.

	4) A main program (Program).

	   This class is the driver for the sample. It creates an application
	   managed by an AppManager, populates it with a few queries, and starts
	   checkpointing on the queries. If the user presses enter, the system
	   will go through a clean shutdown while preserving the query state for
	   resilient queries. This program takes various configuration values, such
	   as the source files, target directory, and event delay from the
	   App.config file.

This solution also contains a program for generating random CSV files that can
be consumed by the CSV input adapter.