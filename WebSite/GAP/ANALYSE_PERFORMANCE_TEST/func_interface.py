import os
import csv
import numpy as np
import pandas as pd
from datetime import timedelta

from datetime import datetime
# ======================================================================================================================


def get_target_dir():

    target_dir = '/GAP_ANALYSIS'

    return target_dir


def write_release_plan(release_plan, plan_num):

    new_schedule = dict()
    count = 0
    for prod_key in release_plan:
        print('Now looking for', prod_key)
        for item in release_plan[prod_key]:
            new_schedule[count] = dict()
            for row in range(item['QUANTITY']):
                lot_schedule = dict()
                lot_schedule['FAB'] = 'SIMTECH'
                lot_schedule['LOT_ID'] = assign_lot_id(row, prod_key, item['RELEASE_SHIFT'])
                lot_schedule['LOT_TYPE'] = 'P'
                lot_schedule['PRODUCT'] = prod_key
                lot_schedule['START_TIME'] = item['RELEASE_TIMING'].strftime("%d/%m/%Y %H:%M:%S")
                lot_schedule['LOT_SIZE'] = 25
                lot_schedule['DUE_DATE'] = item['RELEASE_TIMING'] + get_average_cycle_time_in_hours(prod_key)
                lot_schedule['PRIORITY'] = 0
                new_schedule[count] = lot_schedule
                count += 1

    start_schedule_file = r'C:/Users/user/Documents/TEST_WP24/Test1/Test3/release_plan/wafer_start_schedule' + '_' + str(plan_num) + '.csv'
    with open(start_schedule_file, 'w', newline='') as csvfile:
        fieldnames = ['FAB', 'LOT_ID', 'LOT_TYPE', 'PRODUCT', 'START_TIME', 'LOT_SIZE', 'DUE_DATE', 'PRIORITY']
        writer = csv.DictWriter(csvfile, fieldnames=fieldnames)
        writer.writeheader()
        for index in new_schedule:
            writer.writerow({
                'FAB': new_schedule[index]['FAB'],
                'LOT_ID': new_schedule[index]['LOT_ID'],
                'LOT_TYPE': new_schedule[index]['LOT_TYPE'],
                'PRODUCT': new_schedule[index]['PRODUCT'],
                'START_TIME': new_schedule[index]['START_TIME'].strftime("%d/%m/%Y %H:%M:%S"),
                'LOT_SIZE': new_schedule[index]['LOT_SIZE'],
                'DUE_DATE': new_schedule[index]['DUE_DATE'].strftime("%d/%m/%Y %H:%M:%S"),
                'PRIORITY': new_schedule[index]['PRIORITY']
            })


def write_new_wafer_start_schedule(new_schedule):

    start_schedule_file = os.getcwd() + get_target_dir() + '/input/wafer_start_schedule.csv'

    with open(start_schedule_file, 'w', newline='') as csvfile:
        fieldnames = ['FAB', 'LOT_ID', 'LOT_TYPE', 'PRODUCT', 'START_TIME', 'LOT_SIZE', 'DUE_DATE', 'PRIORITY']
        writer = csv.DictWriter(csvfile, fieldnames=fieldnames)
        writer.writeheader()
        for indx in new_schedule:
            writer.writerow({
                'FAB': new_schedule[indx]['FAB'],
                'LOT_ID': new_schedule[indx]['LOT_ID'],
                'LOT_TYPE': new_schedule[indx]['LOT_TYPE'],
                'PRODUCT': new_schedule[indx]['PRODUCT'],
                'START_TIME': new_schedule[indx]['START_TIME'].strftime("%d/%m/%Y %H:%M:%S"),
                'LOT_SIZE': new_schedule[indx]['LOT_SIZE'],
                'DUE_DATE': new_schedule[indx]['DUE_DATE'].strftime("%d/%m/%Y %H:%M:%S"),
                'PRIORITY': new_schedule[indx]['PRIORITY']
            })


def write_improved_wafer_start_schedule(suggested_start_schedule, target_dir):

    start_schedule_file = os.getcwd() + '/' + target_dir + '/wafer_start_schedule.csv'

    with open(start_schedule_file, 'w', newline='') as csvfile:
        fieldnames = ['FAB', 'LOT_ID', 'LOT_TYPE', 'PRODUCT', 'START_TIME', 'LOT_SIZE', 'DUE_DATE', 'PRIORITY']
        writer = csv.DictWriter(csvfile, fieldnames=fieldnames)
        writer.writeheader()
        for indx in suggested_start_schedule:
            writer.writerow({
                'FAB': suggested_start_schedule[indx]['FAB'],
                'LOT_ID': suggested_start_schedule[indx]['LOT_ID'],
                'LOT_TYPE': suggested_start_schedule[indx]['LOT_TYPE'],
                'PRODUCT': suggested_start_schedule[indx]['PRODUCT'],
                'START_TIME': suggested_start_schedule[indx]['START_TIME'].strftime("%d/%m/%Y %H:%M:%S"),
                'LOT_SIZE': suggested_start_schedule[indx]['LOT_SIZE'],
                'DUE_DATE': suggested_start_schedule[indx]['DUE_DATE'].strftime("%d/%m/%Y %H:%M:%S"),
                'PRIORITY': suggested_start_schedule[indx]['PRIORITY']
            })


def write_recommended_lot_size(lot_size_recommended, target_dir):

    recommended_lot_size_file = os.getcwd() + '/' + target_dir + '/recommended_lot_size.csv'
    with open(recommended_lot_size_file, 'w', newline='') as csvfile:
        fieldnames = ['PRODUCT', 'SUGGESTED_LOT_SIZE']
        writer = csv.DictWriter(csvfile, fieldnames=fieldnames)
        writer.writeheader()
        for product in lot_size_recommended:
            writer.writerow({
                'PRODUCT': product,
                'SUGGESTED_LOT_SIZE': lot_size_recommended[product]
            })


def write_wafer_start_schedule_from_file(wafer_start_schedule_file):

    df_wafer_start_schedule = pd.read_csv(wafer_start_schedule_file)
    start_schedule_file = os.getcwd() + get_target_dir() + '/input/wafer_start_schedule.csv'

    with open(start_schedule_file, 'w', newline='') as csvfile:
        fieldnames = ['FAB', 'LOT_ID', 'LOT_TYPE', 'PRODUCT', 'START_TIME', 'LOT_SIZE', 'DUE_DATE', 'PRIORITY']
        writer = csv.DictWriter(csvfile, fieldnames=fieldnames)
        writer.writeheader()
        for row in range(df_wafer_start_schedule.shape[0]):
            writer.writerow({
                'FAB': df_wafer_start_schedule.loc[row, 'FAB'],
                'LOT_ID': df_wafer_start_schedule.loc[row, 'LOT_ID'],
                'LOT_TYPE': df_wafer_start_schedule.loc[row, 'LOT_TYPE'],
                'PRODUCT': df_wafer_start_schedule.loc[row, 'PRODUCT'],
                'START_TIME': df_wafer_start_schedule.loc[row, 'START_TIME'],
                'LOT_SIZE': df_wafer_start_schedule.loc[row, 'LOT_SIZE'],
                'DUE_DATE': df_wafer_start_schedule.loc[row, 'DUE_DATE'],
                'PRIORITY': df_wafer_start_schedule.loc[row, 'PRIORITY']
            })


def assign_lot_id(row, prod_key, release_shift):

    lot_id = 'LOT_' + str(prod_key) + '_' + str(release_shift) + '_' + str(row)

    return lot_id


def get_average_cycle_time_in_hours(prod_key):

    df_products = pd.read_csv(get_target_directory() + 'input/product.csv')

    for row in range(df_products.shape[0]):
        if df_products.loc[row, 'PRODUCT'] == prod_key:
            hours_in_day = 24
            hours_in_shift = get_shift_duration_in_hours()
            due_hours = df_products.loc[row, 'TARGET_CYCLE_TIME_IN_DAYS']*hours_in_day
            upperbound_num_shift = (np.ceil(df_products.loc[row, 'TARGET_CYCLE_TIME_IN_DAYS'])*hours_in_day/hours_in_shift)
            while upperbound_num_shift*hours_in_shift > df_products.loc[row, 'TARGET_CYCLE_TIME_IN_DAYS']*hours_in_day:
                upperbound_num_shift -= 1

            return timedelta(hours=upperbound_num_shift*hours_in_shift)


def get_shift_duration_in_hours():

    shift_duration_in_hours: int = 8

    return shift_duration_in_hours


def get_wafer_start_schedule_file():

    source_file = get_target_dir() + '/input/wafer_start_schedule.csv'

    return source_file


def write_new_configuration(new_configuration):

    new_config_file = os.getcwd() + get_target_dir() + get_model_config_file()

    with open(new_config_file, 'w', newline='') as csvfile:
        fieldnames = ['PARAMETER_NAME', 'PARAMETER_VALUE']
        writer = csv.DictWriter(csvfile, fieldnames=fieldnames)
        writer.writeheader()
        for indx in new_configuration:
            writer.writerow({
                'PARAMETER_NAME': new_configuration[indx]['PARAMETER_NAME'],
                'PARAMETER_VALUE': new_configuration[indx]['PARAMETER_VALUE']
            })


def get_model_config_file():

    model_config_file = '/input/model_config.csv'

    return model_config_file


def write_planned_vs_obtained_performance_in_lateness(planned_vs_predicted_delivery, start_datetime,
                                                      iteration, target_dir):

    compare_performance_file = os.getcwd() + '/' + target_dir + '/display_table_' + str(iteration) + '.csv'

    with open(compare_performance_file, 'w', newline='') as csvfile:
        fieldnames = ['EntityName', 'SubEntity', 'DeliveryDate', 'PlannedDeliveryWorkOrders', 'CompletedDeliveryWorkOrders']
        writer = csv.DictWriter(csvfile, fieldnames=fieldnames)
        writer.writeheader()
        for product in planned_vs_predicted_delivery.keys():
            for due_day in planned_vs_predicted_delivery[product].keys():
                writer.writerow({
                                'EntityName': product,
                                'SubEntity': product,
                                'DeliveryDate': (start_datetime + timedelta(days=due_day)).strftime("%m/%d/%Y %H:%M:%S"),
                                'PlannedDelivery': planned_vs_predicted_delivery[product][due_day]['PLANNED'],
                                'CompletedWorkOrders': planned_vs_predicted_delivery[product][due_day]['PREDICTED']
                })


def get_last_result(last_iteration_file):

    results = pd.read_csv(last_iteration_file)
    iterations = []
    for row in range(np.shape(results)[0]):
        iterations.append(results.loc[row, 'ITERATION'])
