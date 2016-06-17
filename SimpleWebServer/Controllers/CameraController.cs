using Devkoes.Restup.WebServer.Attributes;
using Devkoes.Restup.WebServer.Models.Schemas;
using Devkoes.Restup.WebServer.Rest.Models.Contracts;
using SimpleWebServer.DataProvider;
using SimpleWebServer.model;
using System;

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
    }
}
