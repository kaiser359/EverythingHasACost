using UnityEngine;
using TMPro;
using System.Collections.Generic;

public enum Effect
{
    None = 0,
    Wave = 1,
    Shake = 2,
    Hightlight = 3,
}

public class AnimateDialogueText : MonoBehaviour
{
    public TMP_Text dialogueText;
    [HideInInspector] public Dictionary<int, char> effectIndicatorLookup;

    private Effect currentEffect;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //dialogueText = GetComponent<TMP_Text>();
    }

    private void Wave(Vector3[] vertices, int vertexIndex, int charIndex)
    {
        // effect parameters
        float waveFrequency = 6f;
        float waveAmplitude = 5f;

        float offsetY = Mathf.Sin(Time.unscaledTime * waveFrequency + charIndex) * waveAmplitude;
        Debug.Log("offsetY: " + offsetY);

        // effect application
        vertices[vertexIndex + 0].y += offsetY;
        vertices[vertexIndex + 1].y += offsetY;
        vertices[vertexIndex + 2].y += offsetY;
        vertices[vertexIndex + 3].y += offsetY;
    }

    private void Shake(Vector3[] vertices, int vertexIndex)
    {
        // effect parameters
        float shakeMagnitude = 2f;

        float offsetX = Random.Range(-shakeMagnitude, shakeMagnitude);
        float offsetY = Random.Range(-shakeMagnitude, shakeMagnitude);
        
        // effect application
        vertices[vertexIndex + 0].x += offsetX;
        vertices[vertexIndex + 0].y += offsetY;
        vertices[vertexIndex + 1].x += offsetX;
        vertices[vertexIndex + 1].y += offsetY;
        vertices[vertexIndex + 2].x += offsetX;
        vertices[vertexIndex + 2].y += offsetY;
        vertices[vertexIndex + 3].x += offsetX;
        vertices[vertexIndex + 3].y += offsetY;
    }

    void Update()
    {
        UpdateText();
    }

    // Update is called once per frame
    public void UpdateText()
    {
        dialogueText.ForceMeshUpdate();
        var textInfo = dialogueText.textInfo;

        // reset current effect at the start of each update loop
        currentEffect = Effect.None;

        // loop through every character in the text and animate its vertices
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];

            //// skip effect indicators
            //// effect indicator start
            //if (charInfo.character == '{')
            //{
            //    isReadingEffect = true;
            //    break;
            //}

            //if (charInfo.character == '}')
            //{
            //    isReadingEffect = false;
            //    break;
            //}

            //// reading effect indicator
            //if (isReadingEffect)
            //{
            //    switch (charInfo.character)
            //    {
            //        case 'w':
            //            currentEffect = Effect.Wave;
            //            break;
            //        case 's':
            //            currentEffect = Effect.Shake;
            //            break;
            //        default:
            //            currentEffect = Effect.None;
            //            break;
            //    }

            //    break;
            //}

            //// debug where effects are placed in the text
            //foreach (KeyValuePair<int, char> entry in effectIndicatorLookup)
            //{
            //    Debug.Log("effect at " + entry.Key + ": " + entry.Value);
            //}

            // switch current effect based on the effect indicator lookup
            if (effectIndicatorLookup.TryGetValue(i, out char effectIndicator))
            {
                Debug.Log("effect indicator at " + i + ": " + effectIndicator);

                switch (effectIndicator)
                {
                    case 'w':
                        currentEffect = Effect.Wave;
                        break;
                    case 's':
                        currentEffect = Effect.Shake;
                        break;
                    default:
                        currentEffect = Effect.None;
                        break;
                }
            }

            // skip invisible characters (like spaces)
            if (!charInfo.isVisible)
                continue;

            // skip characters that don't have an effect
            if (currentEffect == Effect.None)
                continue;


            var vertexIndex = charInfo.vertexIndex;
            var materialIndex = charInfo.materialReferenceIndex;
            var vertices = textInfo.meshInfo[materialIndex].vertices;

            // animate the vertices of the character to create a wave effect
            switch (currentEffect)
            {
                case Effect.Wave:
                    Wave(vertices, vertexIndex, i);
                    break;
                case Effect.Shake:
                    Shake(vertices, vertexIndex);
                    break;
            }
        }

        // update the mesh with the new vertex positions
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            dialogueText.UpdateGeometry(meshInfo.mesh, i);
        }
    }
}
