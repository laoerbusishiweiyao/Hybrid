using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Chaos
{
    public sealed class WebMessageSample : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        private void Awake()
        {
            // GameObject.Find("/Canvas/SendButton").GetComponent<Button>().onClick.AddListener(() => { WebSession.Default.Send(new UnityInitialized()); });
            //
            // GameObject.Find("/Canvas/SendAsyncButton").GetComponent<Button>().onClick.AddListener(async () =>
            // {
            //     var start = DateTime.Now;
            //     var response = await WebSession.Default.SendAsync(new BrowserInformationRequest());
            //     GameObject.Find("/Canvas/WebMessageConsole").GetComponent<TextMeshProUGUI>().text = $"received response({(DateTime.Now - start).TotalMilliseconds}ms): {response}\n";
            // });
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"{name} => OnPointerClick");
            GameObject.Find("/Canvas/WebMessageConsole").GetComponent<TextMeshProUGUI>().text += $"{name}(PointerClick)" + Environment.NewLine;

            GetComponent<MeshRenderer>().material.color = Color.orange;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log($"{name} => OnPointerDown");
            GameObject.Find("/Canvas/WebMessageConsole").GetComponent<TextMeshProUGUI>().text += $"{name}(OnPointerDown)" + Environment.NewLine;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Debug.Log($"{name} => OnPointerUp");
            GameObject.Find("/Canvas/WebMessageConsole").GetComponent<TextMeshProUGUI>().text += $"{name}(OnPointerUp)" + Environment.NewLine;
        }
    }
}