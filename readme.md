# Introduction

This project consists of a number of components that are used to retrieve and store data collected by a Homewizard device.  The data that is stored is exposed via a simple API.
This is done because the (free) Homewizard App only keeps track of the data from the last 12 months.

# Components


# Deployment

## Deploy InfluxDB

Apply the `deployment.yaml` file that can be found in the `./influxdb` folder.  This deployment will deploy influxdb in the `influxdb` namespace.

Deploy using this command:

```
kubectl apply -f deployment.yaml
```

## Data Retriever

The Data Retriever component is responsible for retrieving the Homewizard data and store it in the InfluxDB database.