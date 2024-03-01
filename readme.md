# Introduction

This project consists of a number of components that are used to retrieve and store data collected by a Homewizard device.  The data that is stored is exposed via a simple API.
This is done because the (free) Homewizard App only keeps track of the data from the last 12 months.

# Components


# Build and Deploy

## Deploy InfluxDB

Apply the `deployment.yaml` file that can be found in the `./influxdb` folder.  This deployment will deploy influxdb in the `influxdb` namespace.

Deploy using this command:

```
kubectl apply -f deployment.yaml
```

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


## Building for ARM32 devices

If you want to run the image on a Raspberry PI, you need to use the `Dockerfile-arm32` for building the image.

If you build this container for arm32 on a Windows system, [use docker buildx to build the image](https://docs.docker.com/build/install-buildx/).

You also need the required emulators.  Find information on how to install them [here](https://docs.docker.com/build/building/multi-platform/#build-and-run-multi-architecture-images).

Make sure that you have the linux/arm/v7 emulator.  Verify this by executing `docker buildx ls`.

Additional background information for this can be found [here](https://github.com/dotnet/dotnet-docker/blob/main/samples/dotnetapp/README.md#build-an-image-for-arm32-and-arm64).

Once everything is in place, build the image using this command:

```
docker buildx build . -f .\Dockerfile-arm32 -t homewizard-datacollector:<tag>
```

## Push to dockerhub

- Build the container locally
- Tag the container so that it can be pushed to dockerhub:
  ```
  docker tag homewizard-datacollector:001 docker.io/fgheysels/homewizard-datacollector:001
  ```
- Make sure to be logged in with Docker Hub via `docker login`
- Push the image to the repository `docker push fgheysels/homewizard-datacollector:0.0.1`

- Pull image `docker pull docker.io/fgheysels/homewizard-datacollector:0.0.1`