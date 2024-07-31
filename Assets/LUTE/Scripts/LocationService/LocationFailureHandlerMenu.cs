using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoGaCulture.LUTE
{
    public class LocationFailureHandlerMenu : MonoBehaviour
    {
        [SerializeField] protected Canvas locationFailureMenuGroup;
        [SerializeField] protected CanvasGroup buttonMenuGroup;

        protected static bool locationFailureMenuActive = false;

        protected List<FailedLocatioNode> failedLocationNodes = new List<FailedLocatioNode>();
        protected LTDescr fadeTween; //Used for fading menu
        protected LocationFailureHandlerMenu instance;
        protected List<Button> nodeButtons = new List<Button>();
        protected virtual void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            if (transform.parent == null)
            {
                GameObject.DontDestroyOnLoad(this);
            }
            else
            {
                Debug.LogError("LocationFailureHandlerMenu should be a root object in the scene hierarchy otherwise it cannot be preserved across scenes.");
            }
        }

        protected virtual void Start()
        {
            if (!locationFailureMenuActive)
            {
                //locationFailureMenuGroup.enabled = false;
                buttonMenuGroup.alpha = 0.0f;
            }
        }

        protected void OnEnable()
        {
            LocationServiceSignals.OnLocationFailed += OnLocationFailed;
        }

        protected void OnDisable()
        {
            LocationServiceSignals.OnLocationFailed -= OnLocationFailed;
        }

        protected virtual void OnLocationFailed(FailureMethod failureMethod, Node relatedNode)
        {
            if (failureMethod == null || relatedNode == null)
            {
                return;
            }

            var newFailedLocationNode = new FailedLocatioNode(failureMethod.QueriedLocation, relatedNode, "Location Not Found...");
            failedLocationNodes.Add(newFailedLocationNode);
        }

        public void ToggleMenu()
        {
            if (fadeTween != null)
            {
                LeanTween.cancel(fadeTween.id, true);
                fadeTween = null;
            }

            foreach (var failedLocationNode in failedLocationNodes)
            {
                var button = nodeButtons.Find(x => x.GetComponentInChildren<TextMeshProUGUI>().text.Contains(failedLocationNode.relatedNode._NodeName));
                if (button == null)
                {
                    CreateNodeButton(failedLocationNode);
                }
            }

            if (locationFailureMenuActive)
            {
                //Fade menu out
                LeanTween.value(buttonMenuGroup.gameObject, buttonMenuGroup.alpha, 0f, 0.4f)
        .setEase(LeanTweenType.easeOutQuint)
        .setOnUpdate((t) =>
        {
            buttonMenuGroup.alpha = t;
        }).setOnComplete(() =>
        {
            buttonMenuGroup.alpha = 0f;
            //locationFailureMenuGroup.enabled = false;
        });
            }
            else
            {
                //Fade menu in
                //locationFailureMenuGroup.enabled = true;
                LeanTween.value(buttonMenuGroup.gameObject, buttonMenuGroup.alpha, 1f, 0.4f)
        .setEase(LeanTweenType.easeOutQuint)
        .setOnUpdate((t) =>
        {
            buttonMenuGroup.alpha = t;
        }).setOnComplete(() =>
        {
            buttonMenuGroup.alpha = 1f;
        });
            }
            locationFailureMenuActive = !locationFailureMenuActive;
        }

        public void PlayNode(Node node)
        {
            if (node != null)
            {
                node.StartExecution();
                failedLocationNodes.RemoveAll(x => x.relatedNode == node);
                var nodeButton = nodeButtons.Find(x => x.GetComponentInChildren<TextMeshProUGUI>().text.Contains(node._NodeName));
                if (nodeButton != null)
                {
                    nodeButtons.Remove(nodeButton);
                    Destroy(nodeButton.gameObject);
                }
                ToggleMenu();
            }
        }

        protected void CreateNodeButton(FailedLocatioNode failedLocationNode)
        {
            var newButton = Resources.Load<GameObject>("Prefabs/FailedNodeButton");
            if (newButton != null)
            {
                var button = Instantiate(newButton, buttonMenuGroup.transform);
                button.GetComponentInChildren<TextMeshProUGUI>().text = failedLocationNode.relatedNode._NodeName + "@" + failedLocationNode.failedLocation.Key;
                button.GetComponent<Button>().onClick.AddListener(() => PlayNode(failedLocationNode.relatedNode));
                nodeButtons.Add(button.GetComponent<Button>());
            }
        }
    }

    [System.Serializable]
    public class FailedLocatioNode
    {
        public LocationVariable failedLocation;
        public Node relatedNode;
        public string failureMessage;

        public FailedLocatioNode(LocationVariable failedLocation, Node relatedNode, string failureMessage)
        {
            this.failedLocation = failedLocation;
            this.relatedNode = relatedNode;
            this.failureMessage = failureMessage;
        }
    }
}
