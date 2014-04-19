﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using System.Linq.Expressions;
using System.Collections;
using System.Text.RegularExpressions;

namespace KSPAPIExtensions.PartMessage
{
    /// <summary>
    /// Apply this attribute to any method you wish to receive messages. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class PartMessageListener : Attribute, IPartMessageListenerV1
    {
        public PartMessageListener(Type delegateType, PartRelationship relations = PartRelationship.Self, GameSceneFilter scenes = GameSceneFilter.Any)
        {
            if (delegateType == null || !delegateType.IsSubclassOf(typeof(Delegate)))
                throw new ArgumentException("Message is not a delegate type");
            if (delegateType.GetCustomAttributes(typeof(PartMessageDelegate), true).Length == 0)
                throw new ArgumentException("Message does not have the PartMessageDelegate attribute");

            this.DelegateType = delegateType;
            this.Scenes = scenes;
            this.Relations = relations;
        }

        /// <summary>
        /// The delegate type that we are listening for.
        /// </summary>
        public Type DelegateType { get; private set; }

        /// <summary>
        /// Scene to listen for message in. Defaults to All.
        /// </summary>
        public GameSceneFilter Scenes { get; private set; }

        /// <summary>
        /// Filter for relation between the sender and the reciever.
        /// </summary>
        public PartRelationship Relations { get; private set; }
    }

    /// <summary>
    /// Marker attribute to apply to events using part messages.
    /// </summary>
    [AttributeUsage(AttributeTargets.Event)]
    public class PartMessageEvent : Attribute
    {
    }

    /// <summary>
    /// The attribute to be applied to a delegate to mark it as a PartMessageDelegate type.
    /// 
    /// To use the message, define an event within a Part or PartModule that uses this delegate.
    /// </summary>
    [AttributeUsage(AttributeTargets.Delegate)]
    public class PartMessageDelegate : Attribute, IPartMessageDelegateV1
    {
        public PartMessageDelegate(Type parent = null, bool isAbstract = false)
        {
            if (parent != null)
            {
                if (!parent.IsSubclassOf(typeof(Delegate)))
                    throw new ArgumentException("Parent is not a delegate type");
                if (parent.GetCustomAttributes(typeof(PartMessageDelegate), true).Length != 1)
                    throw new ArgumentException("Parent does not have the PartMessageDelegate attribute");
            }
            this.Parent = parent;
            this.IsAbstract = isAbstract;
        }

        /// <summary>
        /// Often there is a heirachy of events - with more specific events and encompasing general events.
        /// Define a general event as the parent in this instance and any listeners to the general event
        /// will also be notified. Note that the arguments in this situation are expected to be a truncation
        /// of the argument list for this event.
        /// </summary>
        public Type Parent { get; private set; }

        /// <summary>
        /// This event is considered abstract - it should not be sent directly but should be sent from one of the child events.
        /// </summary>
        public bool IsAbstract { get; private set; }
    }

    /// <summary>
    /// Interface to implement on things that aren't either Parts or PartModules to enable them to send/recieve messages
    /// using the event system as a proxy for an actual part. This interface is not required, however if not implemented
    /// the part recievers will not be able to filter by source relationship.
    /// You will need to call PartMessageService.Register(object) in the Awake method or constructor.
    /// </summary>
    public interface PartMessagePartProxy
    {
        Part ProxyPart { get; }
    }

    /// <summary>
    /// A filter method for outgoing messages. This is called prior to delivery of any messages. If the method returns true
    /// then the message is considered handled and will not be delivered.
    /// 
    /// Information about the source of the message is avaiable from the ICurrentEventInfo as usual, this is passed as an argument for convenience.
    /// </summary>
    /// <returns>True if the message is considered handled and is not to be delivered.</returns>
    public delegate bool PartMessageFilter(ICurrentEventInfo message);

}
