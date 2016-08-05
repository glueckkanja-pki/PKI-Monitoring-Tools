# PKI-Monitoring-Tools
A collection of tools for Microsoft Windows that monitor the health of a Public Key Infrastructure (PKI)

## Setup

All tools are written in C# and use the logging framework [log4net](https://logging.apache.org/log4net/) for output. Look at the [log4net config examples](https://logging.apache.org/log4net/release/config-examples.html) or [log4net documentation](https://logging.apache.org/log4net/release/manual/configuration.html) to understand how to configure the output in each application's app.config file. The default configuration usually logs to console, which also allows you to pipe the output into a file. log4net offers a great range of possibilities, though, for example email notifications.

A best practice for most tools is to set up a Scheduled Task for each monitored object. The output is logged to a CSV file, which can be read by the corporate's monitoring solution. Alternatively or additionally, the tools may write an email to the responsible person in case of an incident.

## Tools

* **CheckHttp**: Checks whether a web site is up and running (returns HTTP 200). Used to ensure availability of AIAs.
* **CheckSSLCert**: Accesses an HTTPS URL and checks whether the SSL certificate is still valid. Warns *before* the SSL certificate expires.

## Support

Please open an issue if you have problems using the monitoring tools or think you have found a bug. Professional support, including tool setup and general PKI consulting, is available from [Glück & Kanja](https://www.glueckkanja.com/).

## Licenses

All tools are available under the [AGPL](LICENSE). 

Some of the dependencies are additionally available under different licenses:
* [log4net](https://logging.apache.org/log4net/) is available under the [Apache License, Version 2.0](https://logging.apache.org/log4net/license.html)
* [SharpZipLib](https://icsharpcode.github.io/SharpZipLib/) is available under the GPL.

## Contributing

You may write documentation and source code, pull requests are welcome! You need to provide your contributions under the terms of a liberal license like the X11 License, as we consider additional licensing options beside the AGPL for special cases or the future.
