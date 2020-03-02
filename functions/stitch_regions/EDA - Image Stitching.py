import argparse, cv2, imutils, os
import numpy as np
from imutils import paths


def create_stitich():
    in_path = os.path.join('..', 'data', 'interim', 'frames', '2019-08-13-1700') # 4Dec18 Ubir Geese
    out_path = os.path.join('..', 'data', 'interim', 'stitched')

    image_paths = sorted(list(paths.list_images(in_path)))
    print(image_paths)

    images = []

    for image_path in image_paths:
        #if 'DJI_0455' in image_path or 'DJI_0456' in image_path:
        image = cv2.imread(image_path)
        images.append(image)

    stitcher = cv2.createStitcher() if imutils.is_cv3() else cv2.Stitcher_create()
    (status, stitched) = stitcher.stitch(images)
    file_name = '{0}.jpg'.format(in_path.split(os.sep)[-1])

    out_path = os.path.join(out_path, file_name)

    if status == 0:
        cv2.imwrite(out_path, stitched)

        cv2.imshow("Stitched", stitched)
        cv2.waitKey(0)
    else:
        print("[INFO] image stitching failed ({})".format(status))