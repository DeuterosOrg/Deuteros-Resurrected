using Godot;
using Godot.Collections;
using System;
using System.IO;

namespace Deuteros.Code.Platform.Helpers
{
	public partial class SpriteManager : Node
	{
		public static Dictionary<string, Texture2D> ImageCache { get; set; }

		/// <summary>
		/// Loads images from cache, or loads from the filesystem and assigns to texturerect
		/// </summary>
		/// <param name="path">user://my_image.png</param>
		/// <param name="textureRect">user://my_image.png</param>
		public static TextureRect LoadImageToTextureRect(string path, TextureRect textureRect)
		{
			// Apply it to the TextureRect
			textureRect.Texture = LoadImage(path);

			return textureRect;
		}

		/// <summary>
		/// Loads images from cache, or loads from the filesystem
		/// </summary>
		/// <param name="path">user://my_image.png</param>
		public static Texture2D LoadImage(string path)
		{
			if (ImageCache.TryGetValue(path, out Texture2D texture))
				return texture;

			return GD.Load<Texture2D>(path);
		}
	}
}