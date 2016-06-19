using CameraController.DataProvider;
using CameraController.model;
using Restup.Webserver.Attributes;
using Restup.Webserver.Models.Contracts;
using Restup.Webserver.Models.Schemas;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace CameraController.Controllers
{

    [RestController(InstanceCreationType.Singleton)]
    public sealed class CameraServiceController
    {
    
        CameraDataProvider dataProvider { get; set; }

        public CameraServiceController()
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

            try
            {
                Guid gid = Guid.Parse(id);
                Camera camera = dataProvider.ReadCamera(gid);
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible, MSIE 11, Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko");
                        client.DefaultRequestHeaders.Add("Accept", "image/jpeg");
                        string cameraUrl = string.Format("http://{0}{1}", camera.hostname, camera.imageUrl);
                        var response = client.GetAsync(cameraUrl).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            // by calling .Result you are performing a synchronous call
                            var responseContent = response.Content;
                            byte[] result = responseContent.ReadAsByteArrayAsync().Result;
                            return (GetResponse)new GetResponse(GetResponse.ResponseStatus.OK, result)
                                .addHeader("Content-Type", "image/jpeg")
                                .addHeader("Cache-Control", "no-cache")
                                .addHeader("Pragma", "no-cache");

                        }
                        else
                        {
                            return new GetResponse(GetResponse.ResponseStatus.NotFound);
                        }
                    }
                }
                catch ( Exception e )
                {
                    throw new InvalidOperationException(e.Message);
                }
            }
            catch (FormatException)
            {
                throw new ArgumentException("Invalid UUID");
            }
        }
    }
}
