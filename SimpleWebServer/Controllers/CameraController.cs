using Devkoes.Restup.WebServer.Attributes;
using Devkoes.Restup.WebServer.Models.Schemas;
using Devkoes.Restup.WebServer.Rest.Models.Contracts;
using SimpleWebServer.DataProvider;
using SimpleWebServer.model;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace SimpleWebServer.Controllers
{

    [RestController(InstanceCreationType.Singleton)]
    public sealed class CameraController
    {
    
        CameraDataProvider dataProvider { get; set; }

        public CameraController()
        {
            this.dataProvider = new CameraDataProvider();
        }

        [UriFormat("/camera")]
        public IPostResponse CreateCamera([FromContent] Camera camera)
        {
            // Always reset the |Guid even if one is set
            camera.id = Guid.NewGuid();

            Camera result = dataProvider.CreateCamera(camera);
            if ( result == null )
            {
                return new PostResponse(PostResponse.ResponseStatus.Conflict);
            }

            return new PostResponse(PostResponse.ResponseStatus.Created, "worked", result);
        }


        [UriFormat("/camera/{id}")]
        public IGetResponse GetCamera(String id)
        {
            if ( id == null )
            {
                throw new ArgumentException( "Invalid ID parameter");
            }
            try
            {
                Guid gid = Guid.Parse(id);

                Camera camera = dataProvider.ReadCamera(gid);
                if (camera != null)
                {
                    return new GetResponse(GetResponse.ResponseStatus.OK, camera);
                }
                return new GetResponse(GetResponse.ResponseStatus.NotFound, null);
            }
            catch ( FormatException )
            {
                throw new ArgumentException("ID must be a valid guid");
            }
        }

        [UriFormat("/camera")]
        public IPutResponse UpdateCamera([FromContent] Camera cameraToUpdate)
        {
            Camera updatedCamera = dataProvider.UpdateCamera(cameraToUpdate);

            if (updatedCamera == null)
            {
                return new PutResponse(PutResponse.ResponseStatus.NotFound);
            }

            return new PutResponse(PutResponse.ResponseStatus.OK, updatedCamera);
        }

        [UriFormat("/camera/{id}")]
        public IDeleteResponse DeleteCamera(String id)
        {
            if ( !dataProvider.DeleteCamera(id) )
            {
                return new DeleteResponse(DeleteResponse.ResponseStatus.NotFound);
            }

            return new DeleteResponse(DeleteResponse.ResponseStatus.OK);
        }

        [UriFormat("/cameras?start={start}&count={count}")]
        public IGetResponse ListCameras( int start, int count )
        {
            if ( count == 0 )
            {
                count = 25;
            }
            IEnumerable<Camera> cameras = dataProvider.ListCameras(start, count);
            if ( cameras != null )
            {
                return (IGetResponse)new GetResponse(GetResponse.ResponseStatus.OK, cameras)
                    .addHeader("Access-Control-Allow-Origin", "*")
                    .addHeader("Access-Control-Allow-Headers", "x-requested-with ");
            }

            return new GetResponse(GetResponse.ResponseStatus.NotFound);
        }

        [UriFormat("/snapshot/{id}")]
        public IGetResponse GetImage( String id )
        {

            GetResponse getResponse = null;
            try
            {
                Guid gid = Guid.Parse(id);
                Camera camera = dataProvider.ReadCamera(gid);
                using (var client = new HttpClient())
                {
                    var response = client.GetAsync(camera.imageUrl).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        // by calling .Result you are performing a synchronous call
                        var responseContent = response.Content;

                        byte[] result = responseContent.ReadAsByteArrayAsync().Result;
                        getResponse = (GetResponse)new GetResponse(GetResponse.ResponseStatus.OK, result)
                            .addHeader("Content-Type", "image/jpeg");
                
                    }
                }

                return getResponse;
            }
            catch (FormatException)
            {
                throw new ArgumentException("Invalid UUID");
            }
        }
    }
}
