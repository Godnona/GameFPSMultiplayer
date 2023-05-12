using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class PhotonStatus : MonoBehaviour
{
    public string photonStatus;
    public TextMeshProUGUI textStatus;

    void Update()
    {
        this.photonStatus = PhotonNetwork.NetworkClientState.ToString();
        this.textStatus.text = this.photonStatus;

    }
}
