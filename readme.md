# Introduction

The 'homewizard-data' project collects and store energy consumption data (electricity, gas) that is collected by a Homewizard device for an extended period of time. This is done because the (free) Homewizard App only keeps track of the data from the last 12 months.
The data that is stored, is exposed via a simple API.

One of the goals of this project is to see the effect on my electricity bill should I have a home-battery.  This is done by using the stored electricity consumption and returned generated electricity data to simulate different scenario's in an Excel worksheet.  

# Components

![component overview](static/img/overview.jpg)

A Homewizard P1 meter is reading out electricity power meter.
The `datacollector` component that is running in a kubernetes cluster is reading out the P1 meter on a timely interval and stores the readings in an `influxdb` database which is also running in the same kubernetes cluster (but in a different namespace).
The data that is stored in `influxdb` can be accessed via the `energy-api`. 
The `energy-api` is exposed via a `traefik` ingress which makes it possible that clients running outside the kubernetes cluster can access that API.

# How to use the collected data in Excel

With MS Excel, it is possible to retrieve data from a Web API.  This makes it possible to directly query the energy-api from Excel and import the returned data in the worksheet in a tabular format.

## Fetch data from the energy API

Fetch the data via the `From Web` option on the `Data` pane in Excel.

![Fetch data from web](./static/img/excel_fetch_data.jpg)

Enter the URL at which the API is listening, and make sure to specify the `fromDate` and `toDate` query parameters.
Traefik is configured to have an ingress for the API at `energy-api`, so the URL can look like this:

```
http://192.168.1.20/energy-api/electricity/daily?fromDate=2024-03-10&toDate=2024-04-16
```

After confirming the data source, a new dialog pops up that allows you to transform the data:

![Transform Data](./static/img/excel_convert_data.jpg)

Click on the `To Table Convert` button, and specify that

![Table Convert](./static/img/excel_to_table.jpg)

Go with the default values; make sure that no delimiter is specified.

On the dialog that now appears, the data can be transformed:

![Transform Data](./static/img/excel_convert_data2.jpg)
Click on the button in the column-header that allows you to expand each record.  On the dialog that appears, select all columns and click OK.

You'll now see that that the data is available in a tabular format

![Tabular data](./static/img/excel_expanded_data.jpg)
The data can be loaded into Excel.  This is done by clicking on the 'Close & Load' button.

The data is now available in Excel and can be used to do all kind of calculations with it.

# Build and Deploy

## Deploy InfluxDB

Apply the `deployment.yaml` file that can be found in the `./influxdb` folder.  This deployment will deploy influxdb in the `influxdb` namespace.

Deploy using this command:

```
kubectl apply -f deployment.yaml
```

### Create database

Log in to the influxdb pod:

```
kubectl exec -n influxdb --stdin --tty influxdb-0 -- /bin/bash
```

After the above command, you're in the bash shell inside the influxdb container.
Enter the influx command shell by executing `influx`.
At the `influx` command shell, execute the following command to create a database:

```
> CREATE DATABASE "home_energy" WITH DURATION 10000d
```

Verify if the database is now availabe via this command:

```
show databases
```

> Ideally, this is done via init-scripts

## Build and run Data Retriever

The Data Retriever component is responsible for retrieving the Homewizard data and store it in the InfluxDB database.

You can pull the image from [Dockerhub](https://hub.docker.com/r/fgheysels/homewizard-datacollector/tags).

### Build the image for a Windows operating system

- Build the project as a container image

  ```bash
  docker build . -f Dockerfile-amd64 -t homewizard-datacollector:<tag>
  ```

- Run the program

  ```bash
  docker run homewizard-datacollector:001 --network="host"
  ```

  The `--network="host"` argument makes sure that the [container connects to the host's network](https://docs.docker.com/engine/reference/run/#network-settings).


### Building for ARM32 devices

If you want to run the image on a Raspberry PI, you need to use the `Dockerfile-arm32` for building the image.

If you build this container for arm32 on a Windows system, [use docker buildx to build the image](https://docs.docker.com/build/install-buildx/).

You also need the required emulators.  Find information on how to install them [here](https://docs.docker.com/build/building/multi-platform/#build-and-run-multi-architecture-images).

Make sure that you have the linux/arm/v7 emulator.  Verify this by executing `docker buildx ls`.

Additional background information for this can be found [here](https://github.com/dotnet/dotnet-docker/blob/main/samples/dotnetapp/README.md#build-an-image-for-arm32-and-arm64).

Once everything is in place, build the image using this command:

```
docker buildx build . -f .\Dockerfile-arm32 -t homewizard-datacollector:<tag>
```

### Push to dockerhub

- Build the container locally
- Tag the container so that it can be pushed to dockerhub:
  ```
  docker tag homewizard-datacollector:001 docker.io/fgheysels/homewizard-datacollector:001
  ```
- Make sure to be logged in with Docker Hub via `docker login`
- Push the image to the repository `docker push fgheysels/homewizard-datacollector:0.0.1`

- Pull image `docker pull docker.io/fgheysels/homewizard-datacollector:0.0.1`

### Deploy to Kubernetes

Deploy the component on a Kubernetes cluster by simply deploying the deployment manifest.

```
kubectl apply -f .\deploy\k8s\deployment.yaml -n homewizard-data
```

## Build and run energy-api

The Energy API is an API which exposes the Homewizard Energy data that is stored in the InfluxDb database.

You can pull the image from [Dockerhub](https://hub.docker.com/r/fgheysels/homewizard-energy-api/tags).

### Building for ARM32 devices

For detailed information, see the same section on the Data Retriever section.

```
docker buildx build . -f .\Dockerfile-arm32 -t homewizard-energy-api:<tag>
```

### Push to dockerhub

- Build the container locally
- Tag the container so that it can be pushed to dockerhub:
  ```
  docker tag homewizard-energy-api:001 docker.io/fgheysels/homewizard-energy-api:001
  ```
- Make sure to be logged in with Docker Hub via `docker login`
- Push the image to the repository `docker push fgheysels/homewizard-energy-api:0.0.1`

- Pull image `docker pull docker.io/fgheysels/homewizard-energy-api:0.0.1`

### Deploy

Before being able to deploy the deployment manifest, it is possible that some CRD's need to be installed first.
This can be doen via
```
kubectl apply -f https://raw.githubusercontent.com/traefik/traefik/v2.10/docs/content/reference/dynamic-configuration/kubernetes-crd-definition-v1.yml
```