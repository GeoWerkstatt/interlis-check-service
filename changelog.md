# Change Log

This Log shows all major changes and enhancements of INTERLIS webcheck service (ilicop)

## 3.0.74 (2022-09-19)

### Features

- Support REST API
- Communication of HTML-Client switched to RESTful (before: SignalR)
- Support validation against local catalogue files
- Support validation against catalogue files delivered by ZIP

## 2.1.69 (2022-05-11)

### Features

- Support INTERLIS 1 transfer files
- Support validation against own models (ili) delivered by ZIP
- Version- and License information included at client

## 2.1.56 (2022-05-11)

### Fixes

- TOML configuration option fixed

## 2.1.54 (2022-02-28)

### Features

- Support of validation of Geopackage databases (containing an ili2db schema)

## 2.0.46 (2022-02-09)

### Fixes

- fixes an issue where network disconnects stops validations
- fixes an issue with transferfiles containing a blank space

### Feature

- contains new UI design
- Enhanced support of frontend customization
- Improved error logs in the case of unavailable logfiles
- Improved status logs and display

## 0.10.42 (2021-11-09)

### Fixes

- Improved scrollbar position
- fixes status in the case of a connection timeout
- fixes an issue with deletes in the case of an invalid upload
- css issues

### Feature

- Support for automatic deletes of logfiles

## 0.9.29 (2021-09-08)

### Fixes

- Improved wrong content-type/encoding of logfiles
- fixes issue with too large request bodies
- fixes issue with upper case extensions of transferfiles

### Feature

- Improved filename specification for logfiles
- Support for automatically deletes of transferfiles after validations
- Added option to provide path to xtf-logfile to clipboard
- Added controller logs

## 0.9.12 (2021-08-18)

### Fixes

- fixes issue at upload of zip files

### Feature

- Support for download of log- and xtflog-files from client
- Enhanced parameter set for calling ilivalidator
- dockerised
