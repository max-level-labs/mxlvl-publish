using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[ExecuteInEditMode]
public class MxlvlS3PublishObject : MonoBehaviour {

    private IAmazonS3 _s3Client;

    private IAmazonS3 Client
    {
        get
        {
            if (_s3Client == null)
            {
                _s3Client = new AmazonS3Client("AKIAJFC3IX3Y4RXGKPHQ", "mxZDmhD+QqQnYWwMkMksxmE44pvLcOKlCV8ItXZ/", RegionEndpoint.USWest1);
            }
            //test comment
            return _s3Client;
        }
    }

    // Use this for initialization
    void Start()
    {
        UnityInitializer.AttachToGameObject(this.gameObject);
    }

    // Update is called once per frame
    void Update () {
		
	}

    void Awake()
    {
        UnityInitializer.AttachToGameObject(this.gameObject);
    }

    public void PostObject(string fileName, string filePath)
    {
        Debug.Log(fileName + " : " + filePath);
        var stream = new FileStream(filePath,
        FileMode.Open, FileAccess.Read, FileShare.Read);

        var request = new PostObjectRequest()
        {
            Bucket = "mxlvl-web",
            Key = "media/"+ fileName,
            InputStream = stream,
            CannedACL = S3CannedACL.PublicRead
        };

        Client.PostObjectAsync(request, (responseObj) =>
        {
            Debug.Log("HELLO WORLD " + responseObj.Response);
            if (responseObj.Exception == null)
            {
                Debug.Log(string.Format("\nobject {0} posted to bucket {1}",
                responseObj.Request.Key, responseObj.Request.Bucket));
            }
            else
            {
                Debug.Log(string.Format("\n receieved error {0}",
                responseObj.Response.HttpStatusCode.ToString()));
            }
        });
    }
}
