import os
import shutil
from generate_placeholders import make_styled_asset, make_gradient

if __name__ == '__main__':
    make_styled_asset('assets/t-harvest.png', 552, 125, '#EADBC8', '#7A2E1D', '#B87333')
    make_styled_asset('assets/t-loaf.png', 552, 150, '#DFCBB2', '#7A2E1D', '#B87333')
    if os.path.exists('assets/dinner-bell-clock.jpg') and not os.path.exists('assets/dinner-bell-clock.gif'):
        shutil.copy('assets/dinner-bell-clock.jpg', 'assets/dinner-bell-clock.gif')
    if os.path.exists('assets/banner-autumn-vines.png') and not os.path.exists('assets/banner-autumn-vines.gif'):
        shutil.copy('assets/banner-autumn-vines.png', 'assets/banner-autumn-vines.gif')
    print("Additional title and gif assets ready!")

