# Direct Debits App

The Direct Debits App is a web app developed with .NET that supports direct debit and payments functionality for users of Exact Online Accounting Software operating in Ireland. The app was developed in conjunction with my father's business as a joint venture. Unfortunately with the passing of my dad in June 2024 the app can no longer be supported as a commercial product. The current state of the app is that: it functions but has quite a few rough edges.

**NB - fork at your own risk.**

## Functionality

This app provides the following functionality:

* Loads out invoices from Exact
* Allows custom amounts to be set against each invoice
* Process the amounts set against each invoice by "matching" with the invoices in Exact
* Create a "batch" record in the database
* Allows the "batch" record to be converted into a SEPA bank file

## Getting Up and Running

##### Requirements

The following are required to run the app:

* .NET Framework 4.6.1
* An Exact Account (ideally a developer one as opposed to a live account!)
* MS SQL Server

The most recent version of Visual Studio known to work well with the app is Visual Studio 2022.

##### Database Setup

Note1: MS SQL Server 2022 is the latest version to work with the app.

Note2: I don't remember how the EF migrations works. The latest schema is captured in `DirectDebit.Models/migrations/scripts/BaseSchema.sql`

Create a database on the SQL Server called something like `directdebits`. Then run the BaseSchema.sql script to setup the DB schema (technically you should be using the EF Migrations but I honestly don't remember exactly how one goes about this at the time of writing).

##### Exact Online Developer Account

You will need to get an Exact Online developer account (the UK region service Ireland). Then when this is provided you will need to setup an App. Potentially a Test App is sufficient if the Direct Debit App will just be setup for servicing a single Exact account.

The client id and client secret provided when setting up an Exact App are required to be stored in the `DirectDebits.Web/web.config`.

    <add key="ClientId" value="{XXXXXX}" />
    <add key="ClientSecret" value="XXXXXX" />

##### Other Config

Other config in `web.config` to be updated includes:

* `TokenPassPhrase` is for storing the access token at rest - just update with a 32 random value for this. Don't use the default that is currently there.
* The connection string for the `SynergyDbContext` should be updated if the default localhost string is not sufficient (i.e. it needs to point to another IP, the database name is different, etc.)

Most other config probably should not need to be updated.

##### Run in Visual Studio

Set the DirectDebits.Web as the startup project and then run the application.

When running:

* You should encounter the login screen - so login,
* complete the registration - which should create a db user and organization,
* then go to the db and :
  * update the LockouEndDateUtc field for the user to allow then access the application
  * set the HasDirectDebitFeature and/or HasPaymentsFeature for the organization

This should mean you are now fully setup with the application and your developer account.

##### Configuring the Organization

Once the organization is up and running the following settings need to be configured in the app. 

**Invoice Periods**

The defaults should be fine, but if there is a requirement for something other than 30, 60, 90 days they can be configured here.

**Bank Details**

The organization must have a Direct Debits or Payments mandate with their bank. Currently this application supports AIB, Bank of Ireland, and Ulster Bank.  The banking details provided will only be used to populate the bank file so if they are incorrect this will not be apparent in the App - only when the bank file is used. So be aware of that.

##### Exact Configuration

These are important configurations for the app to run correctly. It requires some detailed understanding of Exact Online to understand why the value are the way they are. Ultimately they are just the Trade Journal and General Ledge codes. A example would be:

* BankJournalCode: 20
* TradeJournalCode: 70
* BankGlCode: 15800
* TradeGlCode: 15400
* ClassificationFilterID: None

Note that the ClassificationFilterID is used to select classification filters from Exact Online that allows a nicer approach to querying certain invoices.

##### Low Level Config

Once upon a time the Exact Online API had performance issues for a week and this config was added so certain Organizations would use the API a little differently. It has no purpose other than that and was a rather hacky implementation at the time.

## The Application Structure

The application has a number of projects:

* DirectDebits.Web - this is the web interface that handles all the incoming requests and holds much of the application logic in the various action methods.
* DirectDebits.Core - contains much of the banking data structure and sub routines.
* DirectDebits.ExactClient - functionality for interfacing with the Exact Online Rest and XML apis.
* DirectDebits.Models - Entity Framework data models.
* DirectDebits.Persistence - Entity Framework persistence.
* DirectDebits.Common - Various helpers/utilities.
* DirectDebits.OwinAuthentication - At some point the support Exact Online package that supported auth via OWIN was no longer working (the details are now lost in time) - this is essentially a copy of the required auth functionality.
* DirectDebits.Tests - These tests all worked once upon a time, but since they were never automated into the deployment pipeline, at some point they started to break about the time the application went into maintenance mode.

## Required Roles

To perform the apps functionality, this is a non-exhaustive list of Financial roles a user must have in exact:

- Maintain Financial Master Data
- View General Ledger Reports