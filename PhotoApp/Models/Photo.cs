﻿using System;

namespace PhotoApp.Models
{
    public class Photo
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string FileName { get; set; }
    }
}