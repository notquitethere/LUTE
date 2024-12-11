using System;
using System.Collections;
using System.Linq;
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
        private Texture2D _texture;

        public Texture2D texture
        {
            get => _texture;
            set => _texture = value;
        }

        [SerializeField, Tooltip("The name for this image.")]
        private string _name;

        public string name
        {
            get => _name;
            set => _name = value;
        }

        [SerializeField, Tooltip("The width, in meters, of the image in the real world.")]
        private float _physicalWidth;

        public float width
        {
            get => _physicalWidth;
            set => _physicalWidth = value;
        }

        [SerializeField, Tooltip("Position offset to spawn the object")]
        private Vector3 _position;

        public Vector3 position
        {
            get => _position;
            set => _position = value;
        }

        [SerializeField, Tooltip("Object to spawn")]
        private GameObject _arObject;

        public GameObject arObject
        {
            get => _arObject;
            set => _arObject = value;
        }

        private bool _spawned;

        public bool spawned
        {
            get => _spawned;
            set => _spawned = value;
        }

        public AddReferenceImageJobState jobState { get; set; }
    }

    [SerializeField, Tooltip("The set of images to add to the image library at runtime")]
    private ImageData[] _images;

    private ARTrackedImageManager _trackedImageManager;

    void Awake()
    {
        var xrObject = XRManager.Instance.GetXRObject();
        if (xrObject == null)
        {
            Debug.LogError("XR Object not initialized.");
            return;
        }

        _trackedImageManager = xrObject.GetComponentInChildren<ARTrackedImageManager>();
        if (_trackedImageManager == null)
        {
            Debug.LogError("ARTrackedImageManager not found in XR Object.");
            return;
        }
    }

    private ImageData GetImageByName(string name)
    {
        return _images.FirstOrDefault(image => image.name == name);
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            ImageData image = GetImageByName(trackedImage.referenceImage.name);

            if (image == null)
            {
                Debug.LogError("Image not found: " + trackedImage.referenceImage.name);
                continue;
            }

            Debug.Log("Tracked image added: " + trackedImage.referenceImage.name);

            GameObject arObjectInstance = Instantiate(image.arObject, trackedImage.transform.position + image.position, trackedImage.transform.rotation);
            image.spawned = true;

            XRObjectManager.Instance.AddObject(trackedImage.referenceImage.name, arObjectInstance);
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            ImageData image = GetImageByName(trackedImage.referenceImage.name);

            if (image == null)
            {
                Debug.LogError("Image not found: " + trackedImage.referenceImage.name);
                continue;
            }

            if (!image.spawned)
            {
                GameObject arObjectInstance = Instantiate(image.arObject, trackedImage.transform.position + image.position, trackedImage.transform.rotation);
                image.spawned = true;

                XRObjectManager.Instance.AddObject(trackedImage.referenceImage.name, arObjectInstance);
            }
            else
            {
                GameObject arObjectInstance = XRObjectManager.Instance.GetObject(trackedImage.referenceImage.name);
                if (arObjectInstance != null)
                {
                    arObjectInstance.transform.position = trackedImage.transform.position + image.position;
                    arObjectInstance.transform.rotation = trackedImage.transform.rotation;
                }
            }
        }
    }

    private IEnumerator WaitAndLoad()
    {
        yield return new WaitForSeconds(1);

        if (_trackedImageManager == null)
        {
            Debug.LogError("ARTrackedImageManager is null.");
            Continue();
            yield break;
        }

        _trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        AddImagesToLibrary();
    }

    private void AddImagesToLibrary()
    {
        if (_trackedImageManager.referenceLibrary is MutableRuntimeReferenceImageLibrary mutableLibrary)
        {
            try
            {
                foreach (var image in _images)
                {
                    if (image.texture == null || !image.texture.isReadable)
                    {
                        Debug.LogError($"The texture for image {image.name} is not set or is not readable.");
                        continue;
                    }

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
            Debug.Log("The reference image library is not mutable.");
        }
    }

    public override void OnEnter()
    {
        Debug.Log("OnEnter in TrackImage");

        StartCoroutine(WaitAndLoad());
    }

    public override string GetSummary()
    {
        return "Track Images in XR";
    }
}