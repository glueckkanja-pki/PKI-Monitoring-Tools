# PKI-Monitoring-Tools
A collection of tools for Microsoft Windows that monitor the health of a Public Key Infrastructure (PKI)

## Setup

All tools are written in C# and use the logging framework [log4net](https://logging.apache.org/log4net/) for output. Look at the [log4net config examples](https://logging.apache.org/log4net/release/config-examples.html) or [log4net documentation](https://logging.apache.org/log4net/release/manual/configuration.html) to understand how to configure the output in each application's app.config file. The default configuration usually logs to console, which also allows you to pipe the output into a file. log4net offers a great range of possibilities, though, for example email notifications.

A best practice for most tools is to set up a Scheduled Task for each monitored object. The output is logged to a CSV file, which can be read by the corporate's monitoring solution. Alternatively or additionally, the tools may write an email to the responsible person in case of an incident.

## Tools

* **CheckWebCRL**: Downloads a CRL and checks how long it is still valid. If the validity is below a configurable threshold, a warning is written to stdout.
* **CheckHttp**: Checks whether a web site is up and running (returns HTTP 200). Used to ensure availability of AIAs.
* **CheckSSLCert**: Accesses an HTTPS URL and checks whether the SSL certificate is still valid. Warns *before* the SSL certificate expires.
* **CertWarning**: Queries Microsoft Active Directory Certificate Services for certificates that expire soon.

### CertWarning ###

This tool depends on certadm.dll included on Windows Servers with AD CS installed and probably also comes with some Admin packs. It must be registered (`regsvr32 certadm.dll`) in case it is not already.

CertWarning is more sophisticated than the other three tools and must be configured in the app.config file (or CertWarning.config in the compiled version), possibly additionally with an HTML mail template (body.html per default) and a mapping file (User2Group.txt by default).

## Support

Please open an issue if you have problems using the monitoring tools or think you have found a bug. Professional support, including tool setup and general PKI consulting, is available from [Glück & Kanja](https://www.glueckkanja.com/).

## Licenses

All tools are available under the [AGPL](LICENSE). 

Some of the dependencies are additionally available under different licenses:
* [log4net](https://logging.apache.org/log4net/) is available under the [Apache License, Version 2.0](https://logging.apache.org/log4net/license.html)
* [SharpZipLib](https://icsharpcode.github.io/SharpZipLib/) is available under the [GPL](https://www.gnu.org/licenses/gpl-3.0.txt).
* [NSSWrapper](https://github.com/glueckkanja-pki/NSSWrapper) is available under the [GPL](https://github.com/glueckkanja-pki/NSSWrapper/blob/master/gpl-3.0.md) as well as the [Mozilla Public License](https://github.com/glueckkanja-pki/NSSWrapper/blob/master/LICENSE).

## Contributing

You may write documentation and source code, pull requests are welcome! You need to provide your contributions under the terms of a liberal license like the X11 License, as we consider additional licensing options beside the AGPL for special cases or the future.
