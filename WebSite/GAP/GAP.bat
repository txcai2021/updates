copy wafer_start_schedule.csv ANALYSE_PERFORMANCE\wafer_start_schedule.csv
copy predicted_processing_rate.csv ANALYSE_PERFORMANCE\predicted_processing_rate.csv
cd ANALYSE_PERFORMANCE
python analyse_performance.py wafer_start_schedule.csv predicted_processing_rate.csv