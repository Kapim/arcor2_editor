using Unity;
using Base;
using UnityEngine;
using System.Collections.Generic;
using System;

public abstract class InteractiveObject : Clickable {

    public abstract string GetName();
    public abstract string GetId();
    public abstract void OpenMenu();
    public abstract bool HasMenu();
    public abstract bool Movable();
    public abstract void StartManipulation();
    public abstract void Remove();
    public abstract void Rename(string newName);
    public abstract bool Removable();
    public virtual float GetDistance(Vector3 origin) {
        float minDist = float.MaxValue;
        foreach (Collider collider in Colliders) {
            try {
                Vector3 point;
                if (collider is MeshCollider meshCollider && meshCollider.convex)
                    point = collider.ClosestPoint(origin);
                else 
                    point = collider.ClosestPointOnBounds(origin);

                minDist = Math.Min(Vector3.Distance(origin, point), minDist);
            } catch (UnassignedReferenceException ex) {

            }            

        }
        foreach (Collider2D collider in Colliders2D) {
            Vector3 point = collider.ClosestPoint(origin);
            minDist = Math.Min(Vector3.Distance(origin, point), minDist);
        }
        
        return minDist;
    }

    public virtual void Enable(bool enable) {
        Enabled = enable;
        foreach (Collider c in Colliders)
            c.enabled = enable;
    }

    public List<Collider> Colliders = new List<Collider>();
    public List<Collider2D> Colliders2D = new List<Collider2D>();


}
