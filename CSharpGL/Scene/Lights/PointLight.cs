﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpGL
{
    /// <summary>
    /// 
    /// </summary>
    public class PointLight : LightBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        public PointLight(vec3 position)
        {
            this.WorldPosition = position;
        }

        /// <summary>
        /// 
        /// </summary>
        public vec3 WorldPosition { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override mat4 GetProjectionMatrix()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override mat4 GetViewMatrix()
        {
            throw new NotImplementedException();
        }
    }
}
