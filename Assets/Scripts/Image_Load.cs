using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.TextCore.Text;
public class Image_Load : MonoBehaviour
{
    //https://www.cdc.gov/healthypets/images/pets/cute-dog-headshot.jpg?_=42445
    [Header ("Unity webrequest does not support WebP")]
    public string _url = "https://avatars.githubusercontent.com/u/26628304?v=4";
    public TMPro.TextMeshProUGUI _text;
    public AspectRatioFitter _aspectRatio;
    public Image _image;

    // Start is called before the first frame update

    void Start()
    {
        _image = GetComponent<Image>();
        //_text = GetComponent<TMPro.TextMeshProUGUI>();
        _aspectRatio = GetComponent<AspectRatioFitter>();
        StartCoroutine(GetTexture(_url));
    }

    //Unity Texture2D resize cleans pixel data, heres a custom method
    /// <summary>
    /// Blits source onto temporary texture and reads pixels with new dimensions.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="newWidth"></param>
    /// <param name="newHeight"></param>
    /// <returns></returns>
    public static Texture2D Resize(Texture2D source, int newWidth, int newHeight, FilterMode filterMode = FilterMode.Point)
    {
        //create temporary texture that can be freed up later
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);

        //use same filtering, can be changed to bi/trilinear
        source.filterMode = filterMode;
        rt.filterMode = filterMode;
        
        //blit source onto temp texture and read the pixels using resized dimensions
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);
        Texture2D nTex = new Texture2D(newWidth, newHeight);
        nTex.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0,0);
        nTex.Apply();

        //free up texture from memory
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        return nTex;

    }

    /// <summary>
    /// Uses UnityWebRequest to pull images from the web into texture. 
    /// </summary>
    /// <param name="textureUrl"></param>
    IEnumerator GetTexture(string textureUrl, bool doCompress = true)
    {
        //get texture makes more sense here than WWWForm, more efficient use of memory.
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(textureUrl);
        yield return www.SendWebRequest();

        //handle errors
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error);
            _text.text = www.error;

            //download handler errors stored seperately and override normal errors
            //when present
            if(www.downloadHandler.error != null)
            {
                _text.text = www.downloadHandler.error;
                 Debug.LogError(www.downloadHandler.error);
            
            }
            
        }

        else
        {
            _text.enabled = false;

            //Image needs sprite so converting texture to sprite. 
            Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            Rect rect = new Rect(0, 0, texture.width, texture.height);
            Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100);
            

            //texture cannot be compressed unless it is divisible by 4
            //this should really be solved by just giving it properly formatted images
            //but heres a solution that works in engine
            if(texture.height % 4 != 0 || texture.width % 4 != 0)
            {
                var heightMitigation = texture.height % 4;
                var widthMitigation = texture.width % 4;
                texture = Resize(texture, texture.height - heightMitigation, texture.width - widthMitigation);
            }
            texture.Compress(doCompress);

            //set aspect ratio
            if(texture.height >= texture.width)
            {
                _aspectRatio.aspectRatio = texture.height / texture.width;
            }
            else
            {
                _aspectRatio.aspectRatio = texture.width /texture.height;
            }
            
            
            _image.sprite = sprite;
        }

    }
}


