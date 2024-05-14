using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[OrderInfo("XR",
              "Track Image",
              "Track Images in XR")]
[AddComponentMenu("")]
public class TrackImage : Order
{

    [Serializable]
    public class ImageData
    {
        [SerializeField, Tooltip("The source texture for the image. Must be marked as readable.")]
        Texture2D m_Texture;

        public Texture2D texture
        {
            get => m_Texture;
            set => m_Texture = value;
        }

        [SerializeField, Tooltip("The name for this image.")]
        string m_Name;

        public string name
        {
            get => m_Name;
            set => m_Name = value;
        }

        [SerializeField, Tooltip("The width, in meters, of the image in the real world.")]
        float m_physicalWidth;

        public float width
        {
            get => m_physicalWidth;
            set => m_physicalWidth = value;
        }

        [SerializeField, Tooltip("Position to spawn the object")]
        Vector3 m_Position;

        public Vector3 position
        {
            get => m_Position;
            set => m_Position = value;
        }

        [SerializeField, Tooltip("Object to spawn")]
        GameObject m_Object;

        public GameObject arObject
        {
            get => m_Object;
            set => m_Object = value;
        }


        bool m_Spawned;

        public bool spawned
        {
            get => m_Spawned;
            set => m_Spawned = value;
        }

        /// Position GPS TODO

        public AddReferenceImageJobState jobState { get; set; }
    }

    [SerializeField, Tooltip("The set of images to add to the image library at runtime")]
    ImageData[] m_Images;


    private ImageData getImageByName(string name)
    {
        foreach (var image in m_Images)
        {
            if (image.name == name)
            {
                return image;
            }
        }
        return null;
    }

    ARTrackedImageManager m_TrackedImageManager;

    void Awake()
    {
        m_TrackedImageManager = GameObject.Find("XR").GetComponentInChildren<ARTrackedImageManager>();
    }


    //void OnEnable()
    //{
    //    m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    //}

    //void OnDisable()
    //{
    //    m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        
    //}

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {

            ImageData image = getImageByName(trackedImage.referenceImage.name);

            if(image == null)
            {
                Debug.LogError("Image not found: " + trackedImage.referenceImage.name);
                //return;
            }

            Debug.Log("Tracked image added: " + trackedImage.referenceImage.name);

            //attach the object to the tracked image
            GameObject arObjectInstance = Instantiate(image.arObject, trackedImage.transform.position + image.position, trackedImage.transform.rotation);

            //set the spawned flag to true
            image.spawned = true;

            XRObjectManager.AddObject(trackedImage.referenceImage.name, arObjectInstance);


            //// Give the initial image a reasonable default scale
            //trackedImage.transform.localScale = new Vector3(0.1f, 0.01f, 0.01f);


        }

        foreach (var trackedImage in eventArgs.updated)
        {




            GameObject xrObject = GameObject.Find("XR");


            ImageData image = getImageByName(trackedImage.referenceImage.name);
            //attach the object to the tracked image
            //GameObject arObjectInstance = Instantiate(image.arObject, trackedImage.transform.position, trackedImage.transform.rotation);

            if(image == null)
            {
                Debug.LogError("Image not found: " + trackedImage.referenceImage.name);
                //return;
            }

            //if it's not spawned yet
            if (!image.spawned)
            {
                //spawn the object
                GameObject arObjectInstance = Instantiate(image.arObject, trackedImage.transform.position + image.position, trackedImage.transform.rotation);
                image.spawned = true;

                XRObjectManager.AddObject(trackedImage.referenceImage.name, arObjectInstance);

            }

            //ARTrackedImageManager manager = xrObject.GetComponentInChildren<ARTrackedImageManager>();
        }

        //UpdateTrackedImages(eventArgs.updated);
        //UpdateTrackedImages(eventArgs.added);

        //foreach (var trackedImage in eventArgs.updated)
        //    UpdateInfo(trackedImage);
    }

    //a serialized library that contains the images 
    //public XRReferenceImageLibrary imageLibrary;



    private void UpdateTrackedImages(IEnumerable<ARTrackedImage> trackedImages)
    {
        // If the same image (ReferenceImageName)
        var trackedImage =
            trackedImages.FirstOrDefault(x => x.referenceImage.name == "Mural");
        if (trackedImage == null)
        {
            return;
        }

        if (trackedImage.trackingState != TrackingState.None)
        {
            var trackedImageTransform = trackedImage.transform;
            transform.SetPositionAndRotation(trackedImageTransform.position, trackedImageTransform.rotation);
        }
    }

    IEnumerator waitAbitAndLoad()
    {

       

        yield return new WaitForSeconds(1);
        m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        test();
        
       


        //ARTrackedImageManager aRTrackedImageManager = arObjectInstance.GetComponentInChildren<ARTrackedImageManager>();

        //aRTrackedImageManager.referenceLibrary = imageLibrary;
    }

    public void test()
    {
        //this code gets executed as the order is called
        //some orders may not lead to another node so you can call continue if you wish to move to the next order after this one   
        //Continue();

       

       

        XRHelper.setXRActive(true);
        Debug.Log("AR SESSSION IS:" + ARSession.state.ToString());


        ARTrackedImageManager manager = XRHelper.getXRScript().gameObject.GetComponentInChildren<ARTrackedImageManager>();



        if (manager.referenceLibrary is MutableRuntimeReferenceImageLibrary mutableLibrary)
        {

            try
            {
                foreach (var image in m_Images)
                {
                    //if texture format is not supported, skip this image
                    if (image.texture == null || !image.texture.isReadable)
                    {
                        Debug.LogError($"The texture for image {image.name} is not set or is not readable.");
                        continue;
                    }

                    //if the texture format is not RGBA32, create a new texture with the correct format 

                    if (image.texture.format != TextureFormat.RGBA32)
                    {
                        var newTexture = new Texture2D(image.texture.width, image.texture.height, TextureFormat.RGBA32, false);
                        Graphics.CopyTexture(image.texture, newTexture);
                        image.texture = newTexture;
                    }

                    image.jobState = mutableLibrary.ScheduleAddImageWithValidationJob(image.texture, image.name, image.width);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        else
        {


            Debug.Log($"The reference image library is not mutable.");
        }
    }


    public override void OnEnter()
    {

        Debug.Log("OnEnter in Tracking Images");
      

        StartCoroutine(waitAbitAndLoad());

        Debug.Log("OnEnter exit");

        //Continue();

        //spawn the arObject
        //GameObject arObjectInstance = Instantiate(arObject, Vector3.zero, Quaternion.identity);
        //set the XRReferenceImageLibrary of the "XR Origin (XR Rig)" to the imageLibrary

        //ARTrackedImageManager aRTrackedImageManager = arObjectInstance.GetComponentInChildren<ARTrackedImageManager>();

        //aRTrackedImageManager.referenceLibrary = imageLibrary;

    }

  public override string GetSummary()
  {
        //you can use this to return a summary of the order which is displayed in the inspector of the order
        return "Track Images in XR";
  }
}