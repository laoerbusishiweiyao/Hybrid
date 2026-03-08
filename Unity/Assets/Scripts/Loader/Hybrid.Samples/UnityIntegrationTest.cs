using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Chaos
{
    public sealed class UnityIntegrationTest : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"{name} => OnPointerClick");
            GameObject.Find("/Canvas/WebMessage").GetComponent<TextMeshProUGUI>().text += $"{name}(PointerClick)" + Environment.NewLine;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log($"{name} => OnPointerDown");
            GameObject.Find("/Canvas/WebMessage").GetComponent<TextMeshProUGUI>().text += $"{name}(OnPointerDown)" + Environment.NewLine;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Debug.Log($"{name} => OnPointerUp");
            GameObject.Find("/Canvas/WebMessage").GetComponent<TextMeshProUGUI>().text += $"{name}(OnPointerUp)" + Environment.NewLine;
        }
    }
}