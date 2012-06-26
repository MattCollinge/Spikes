========================================
WcfApplication Sample
========================================

Last change: 7/2/2010

This sample demonstrates the following aspects:

* Input and output adapters that support enqueuing and dequeuing events via WCF
  web service endpoints.
	- IPointInputAdapter and IPointOutputAdapter describe the service contracts.
	- WcfPointInputAdapter and WcfPointOutputAdapter implement the service
	  contracts as well as the corresponding StreamInsight adater contracts.
	- ClientPointInputAdapter and ClientPointOutputAdapter wrap the service
	  channel and provide a simple synchronous surface above the rety logic
	  implemented in the ClientAdapter base class.
* Endpoints are collocated with the StreamInsight server. Notice that the
  sample application collocates the endpoint clients as well which is not
  required.

Note that you need to set the correct URL permissions to be able to create the
web service endpoints. Run the following commands in an admin shell:

netsh http add urlacl url=http://+:8080/OutputAdapter user=domain\username
netsh http add urlacl url=http://+:8080/InputAdapter user=domain\username

