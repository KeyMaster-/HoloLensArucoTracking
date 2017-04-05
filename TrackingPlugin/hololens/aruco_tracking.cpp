#include "aruco_tracking.h"

using namespace std;
using namespace cv;

extern "C" {
	PrintFunc debug;

	Ptr<aruco::Dictionary> dict;

	int img_width, img_height;

		//Using pointers everywhere may get messy, making a class to hold all of this and just having a pointer to one instance at the global level may be useful in the future

	Mat *camera_matrix;
	Mat *dist_coeffs;

	vector<int> *ids;
	vector<vector<Point2f>> *corners;
	vector<float> *corners_flat;

		//Rotation and translation vectors from pose estimation
	vector<Vec3d> *tvecs;
	vector<double> *tvecs_flat;
	vector<Vec3d> *rvecs;
	vector<double> *rvecs_flat;


		//camera_params are the camera focal lengths (x,y), the principal point values (x,y), each from the opencv camera matrix, and then the 5 camera distortion values
	void init(int _img_width, int _img_height, float *camera_params) {
		img_width = _img_width;
		img_height = _img_height;

		dict = aruco::getPredefinedDictionary(aruco::DICT_ARUCO_ORIGINAL);

		ids = new vector<int>();
		corners = new vector<vector<Point2f>>();
		corners_flat = new vector<float>();

		tvecs = new vector<Vec3d>();
		rvecs = new vector<Vec3d>();
		tvecs_flat = new vector<double>();
		rvecs_flat = new vector<double>();

		camera_matrix = new Mat();
		dist_coeffs = new Mat();

		camera_matrix->create(3, 3, CV_64F);
		camera_matrix->at<double>(0, 0) = camera_params[0];
		camera_matrix->at<double>(0, 1) = 0.0;
		camera_matrix->at<double>(0, 2) = camera_params[2];
		camera_matrix->at<double>(1, 0) = 0.0;
		camera_matrix->at<double>(1, 1) = camera_params[1];
		camera_matrix->at<double>(1, 2) = camera_params[3];
		camera_matrix->at<double>(2, 0) = 0.0;
		camera_matrix->at<double>(2, 1) = 0.0;
		camera_matrix->at<double>(2, 2) = 1.0;

		dist_coeffs->create(5, 1, CV_64F);
		dist_coeffs->at<double>(0, 0) = camera_params[4];
		dist_coeffs->at<double>(0, 1) = camera_params[5];
		dist_coeffs->at<double>(0, 2) = camera_params[6];
		dist_coeffs->at<double>(0, 3) = camera_params[7];
		dist_coeffs->at<double>(0, 4) = camera_params[8];
	}

	int detect_markers(unsigned char *unity_img, int* out_ids_len, int** out_ids, float** out_corners, double** out_rvecs, double** out_tvecs) { //pointer to array (int*) of ids, pointer to variable that we'll write array length into
		Mat img = Mat(img_height, img_width, CV_8UC4, unity_img, img_width * 4);
		Mat gray;

		cvtColor(img, gray, CV_RGBA2GRAY);
		
		aruco::detectMarkers(gray, dict, *corners, *ids);

		//write address of elem 0 in ids vector to out_ids (*out_ids = address, since out_ids is just our copy of the c# variable address), write array length to *out_ids_len.
		//Also, ids has to be allocated non-local, otherwise it goes out of scope after this function ends..

		int marker_count = ids->size();
		*out_ids_len = marker_count;
		*out_ids = NULL;
		*out_corners = NULL;
		if (*out_ids_len > 0) {
			aruco::estimatePoseSingleMarkers(*corners, 0.088, *camera_matrix, *dist_coeffs, *rvecs, *tvecs);

			corners_flat->resize(marker_count * 8); //For each marker, we have 4 corner points, each of which are 2 floats. So we need markers * 8 floats overall;
			rvecs_flat->resize(marker_count * 3); // 1 rvec per marker, 3 doubles per Vec3d
			tvecs_flat->resize(marker_count * 3); // Same as for rvecs
			for (int i = 0; i < marker_count; i++) {
					//corners has an array of 4 `Point2f`s for each marker. Since they're continuous, and each Point2f is just 2 floats, we can copy 4 points into 8 floats in our flat array
				memcpy(corners_flat->data() + (i * 8), (*corners)[i].data(), 4 * sizeof(Point2f)); 

					//Copy over rvec and tvec doubles into corresponding flat arrays
				for (int j = 0; j < 3; j++) {
					(*rvecs_flat)[i * 3 + j] = (*rvecs)[i][j];
					(*tvecs_flat)[i * 3 + j] = (*tvecs)[i][j];
				}

			}

			*out_ids = ids->data();
			*out_corners = corners_flat->data();
			*out_rvecs = rvecs_flat->data();
			*out_tvecs = tvecs_flat->data();
		}

		int result = ids->size();
		
		return result;
	}

	void destroy() {
		delete ids;
		delete corners;
		delete corners_flat;

		delete tvecs;
		delete rvecs;
		delete tvecs_flat;
		delete rvecs_flat;

		delete camera_matrix;
		delete dist_coeffs;
	}

	void set_debug_cb(PrintFunc ptr) {
		debug = ptr;
	}
}