using UnityEngine;
using TMPro;

public class TextWave : MonoBehaviour
{
    private TMP_Text dialogueText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dialogueText = GetComponent<TMP_Text>();
    }

    private void Wave(Vector3[] vertices, int vertexIndex, int charIndex)
    {
        // effect parameters
        float waveFrequency = 6f;
        float waveAmplitude = 5f;

        float offsetY = Mathf.Sin(Time.unscaledTime * waveFrequency + charIndex) * waveAmplitude;
        //Debug.Log("offsetY: " + offsetY);

        // effect application
        vertices[vertexIndex + 0].y += offsetY;
        vertices[vertexIndex + 1].y += offsetY;
        vertices[vertexIndex + 2].y += offsetY;
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

        // loop through every character in the text and animate its vertices
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];

            // skip invisible characters (like spaces)
            if (!charInfo.isVisible)
                continue;

            var vertexIndex = charInfo.vertexIndex;
            var materialIndex = charInfo.materialReferenceIndex;
            var vertices = textInfo.meshInfo[materialIndex].vertices;

            // animate the vertices of the character to create a wave effect
            Wave(vertices, vertexIndex, i);
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
