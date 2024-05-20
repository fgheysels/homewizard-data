## Build and deploy energy-api

The Energy API is an API which exposes the Homewizard Energy data that is stored in the InfluxDb database.

You can pull an existing image from [Dockerhub](https://hub.docker.com/r/fgheysels/homewizard-energy-api/tags).

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

Deploy the component on a Kubernetes cluster by simply deploying the deployment manifest.

```
kubectl apply -f .\deploy\k8s\deployment.yaml -n homewizard-data
```

Before being able to deploy the deployment manifest, it is possible that some CRD's need to be installed first.
This can be doen via
```
kubectl apply -f https://raw.githubusercontent.com/traefik/traefik/v2.10/docs/content/reference/dynamic-configuration/kubernetes-crd-definition-v1.yml
```