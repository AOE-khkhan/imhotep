﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

/*! Handles Notifications.
 * How to use the Notification system:
 * 		// Show notification for 10 seconds:
 *      NotificationControl.instance.createNotification("test", new TimeSpan(0,0,10));
 * A notification can also have its own icon:
 *      public Sprite SpriteUnknown; //Set in editor
 *      NotificationControl.instance.createNotification("test", new TimeSpan(0,0,10), s);
*/
public class NotificationControl : MonoBehaviour {

    public GameObject firstNotification;
    public GameObject secondNotification;
    public GameObject thirdNotification;
    public GameObject statusBar;
	public GameObject platform;

    public int angle = 45;

    private List<Notification> notitficationList = new List<Notification>();
    //private List<GameObject> gameObjectNotificationList = new List<GameObject>();

    private int leftPosX = 0;
    private int centerPosX = 0;
    private int rightPosX = 0;

	public static NotificationControl instance { get; private set; }

	public NotificationControl()
	{
		if (instance != null)
			throw( new System.Exception("Cannot create NotificationControl: Only one NotificationControl may exist!" ));

		instance = this;
	}

    // Use this for initialization
    void Start () {
        firstNotification.SetActive(false);
        secondNotification.SetActive(false);
        thirdNotification.SetActive(false);
        this.gameObject.SetActive(false);

		//Find Platform script
		Platform platformScript = platform.GetComponent<Platform>();

        //Calculate left, center and right position for notification bar
        int statusbarWidth = (int)statusBar.GetComponent<RectTransform>().rect.width;
		centerPosX = 0;
		//if (platformScript.getIsRounded ()) { 
			//leftPosX = -statusbarWidth / 4;
			//rightPosX = statusbarWidth / 4;
		//} else {
			int widthCenterScreen = (int)platformScript.getScreenDimensions (UI.Screen.center).x;
			int widthRightScreen = (int)platformScript.getScreenDimensions (UI.Screen.right).x;
			int widthLeftScreen = (int)platformScript.getScreenDimensions (UI.Screen.left).x;
			rightPosX = (widthCenterScreen / 2) + (widthRightScreen / 2);
			leftPosX = (widthCenterScreen / 2) + (widthLeftScreen / 2);
		//}
		Debug.LogWarning(widthRightScreen + " - " + widthLeftScreen);
    }

    // Update is called once per frame
    void Update () {
        bool listChanged = deleteNotificationsIfExpired();
        if (listChanged)
        {
            updateNotificationCenter();
        }

        //Check camera yaw and change x position of notification center
        float cameraYaw = Camera.main.transform.localRotation.eulerAngles.y; // values between 0 and 360
        if(cameraYaw > angle && cameraYaw <= 180)
        {
            this.GetComponent<RectTransform>().localPosition = new Vector2(rightPosX, this.GetComponent<RectTransform>().localPosition.y);
        }
        else if (cameraYaw > 180 && cameraYaw < 360 - angle)
        {
            this.GetComponent<RectTransform>().localPosition = new Vector2(leftPosX, this.GetComponent<RectTransform>().localPosition.y);
        }
        else
        {
            this.GetComponent<RectTransform>().localPosition = new Vector2(centerPosX, this.GetComponent<RectTransform>().localPosition.y);
        }

    }



    public void debug()
    {
        System.Random rnd = new System.Random();
        string s = "Notification " + rnd.Next(1, 99);
        //Notification n = new Notification(s, TimeSpan.FromSeconds(10));
		Notification n = new Notification(s, TimeSpan.Zero);
        createNotification(n);
    }

    public void createNotification(Notification n)
    {
        notitficationList.Add(n);
        updateNotificationCenter();
	}

	//! Convenience overload
	public void createNotification( string text, TimeSpan timeToLive, Sprite notificationSprite = null )
	{
		Notification n;
		if (notificationSprite == null) {
			n = new Notification (text, timeToLive);
		} else {
			n = new Notification (text, timeToLive, notificationSprite);
		}
		createNotification (n);
	}
	
    private bool deleteNotificationsIfExpired()
    {
        bool result = false;
        for(int i = notitficationList.Count - 1; i >= 0; i--) //Foreach dont work
        {
            if(notitficationList[i].ExpireDate < DateTime.Now)
            {
                notitficationList.RemoveAt(i);
                result = true;
            }
        }
        return result;
    }

    //! Called after a notification has been removed or added
    private void updateNotificationCenter()
    {
        if (notitficationList.Count == 0)
        {
            this.gameObject.SetActive(false);
            return;
        }

        firstNotification.SetActive(false);
        secondNotification.SetActive(false);
        thirdNotification.SetActive(false);

        this.gameObject.SetActive(true);

        if(notitficationList.Count > 0)
        {
            firstNotification.SetActive(true);
            setTextAndIconInNotifiaction(firstNotification, notitficationList[0]);
        }
        if (notitficationList.Count > 1)
        {
            secondNotification.SetActive(true);
            setTextAndIconInNotifiaction(secondNotification, notitficationList[1]);
        }
        if (notitficationList.Count > 2)
        {
            thirdNotification.SetActive(true);
            setTextAndIconInNotifiaction(thirdNotification, notitficationList[2]);
        }
    }

    private void setTextAndIconInNotifiaction(GameObject notificationGameObject, Notification n)
    {
        //Set text
        notificationGameObject.GetComponentInChildren<Text>().text = n.Text;

        //Set Icon
        if (n.NotificationSprite != null)
        {
            notificationGameObject.transform.FindChild("Icon").GetComponent<Image>().sprite = n.NotificationSprite;
        }

        //Set reference to notification object
        notificationGameObject.GetComponent<NotificationReference>().notification = n;

    }

    public void deleteNotoficationPressed(GameObject sender)
    {
        
        for (int i = notitficationList.Count - 1; i >= 0; i--)
        {
            if (sender.GetComponent<NotificationReference>().notification == notitficationList[i])
            {
                notitficationList.RemoveAt(i);
                break;
            }
        }
        updateNotificationCenter();
    }
}
