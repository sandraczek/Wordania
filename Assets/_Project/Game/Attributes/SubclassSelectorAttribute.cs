using System;
using UnityEngine;

namespace Wordania.Attributes
{
    /// <summary>
    /// Attribute used to display a dropdown of derived types for a field serialized with [SerializeReference].
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class SubclassSelectorAttribute : PropertyAttribute
    {
    }
}