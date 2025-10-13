#!/usr/bin/env python3
"""
Convert PNG to ICO for Windows application icon
"""

from PIL import Image
import sys

def png_to_ico(png_path, ico_path):
    """Convert PNG to ICO with multiple sizes"""
    try:
        # Load PNG
        img = Image.open(png_path)
        
        # Convert to RGBA if needed
        if img.mode != 'RGBA':
            img = img.convert('RGBA')
        
        # Create icon with multiple sizes
        icon_sizes = [(16, 16), (32, 32), (48, 48), (64, 64), (128, 128), (256, 256)]
        
        # Save as ICO with all sizes
        img.save(ico_path, format='ICO', sizes=icon_sizes)
        
        print(f"Icon created successfully: {ico_path}")
        return True
        
    except ImportError:
        print("PIL/Pillow not installed. Installing...")
        import subprocess
        subprocess.check_call([sys.executable, "-m", "pip", "install", "Pillow"])
        # Retry
        return png_to_ico(png_path, ico_path)
    except Exception as e:
        print(f"Error creating icon: {e}")
        return False

if __name__ == "__main__":
    png_path = "assets/DPS_Meter_Logo_White.png"
    ico_path = "frontend/Assets/app_icon.ico"
    
    print("Converting PNG to ICO format...")
    success = png_to_ico(png_path, ico_path)
    sys.exit(0 if success else 1)

