Cedar
===

[![Build status](https://ci.appveyor.com/api/projects/status/4ck4andqsnnrbes1)](https://ci.appveyor.com/project/damianh/cedar) 

An opinionated suite of components to help build CQRS/Event Sourced/Domain Driven Design applications based on NEventStore

 1. Command Handling Owin middleware - supports exception serialization and command versioning strategies).
 2. Durable Commit Dispatcher - used for subscribing to Domain Events to safely and reliably create projections.
 3. Domain Model - an updated and tweaked version CommonDomain.
 4. Query Owin Middleware - allows easy defining of query endpoints and a strategy for backwards compatibility.
 5. ProcessManager
 6. Testing library - helpers to test aggregates, projections, processes and aid with subcutanous testing.

A sample application and documentation will be forthcoming when APIs etc mature a bit.

[CI Feed](https://www.myget.org/F/dh/)
