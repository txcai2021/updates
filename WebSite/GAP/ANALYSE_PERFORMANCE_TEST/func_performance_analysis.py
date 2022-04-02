import os
import shutil
import subprocess
import csv
import numpy as np
import pandas as pd
from datetime import datetime, timedelta
from func_interface import get_model_config_file
from func_interface import get_target_dir
# --------------------------------------------------------------------------------------------------------------


# def rearrange_wafer_start_schedule(wafer_start_schedule_file):
#
#     wafer_start_schedule = get_wafer_start_schedule(wafer_start_schedule_file)
#     timestamp_earliest_release, release_gap_in_minutes, due_date_gap_in_minutes = get_scheduling_time_parameter_constraints(wafer_start_schedule)
#
#     new_wafer_start_schedule = {
#                                     job: get_new_release_time(
#                                             wafer_start_schedule[job]['START_TIME'],
#                                             wafer_start_schedule[job]['DUE_DATE'],
#                                             timestamp_earliest_release,
#                                             release_gap_in_minutes,
#                                             due_date_gap_in_minutes)
#                                     for job in wafer_start_schedule
#                                 }
#
#     new_schedule_rearranged = get_new_wafer_start_schedule(wafer_start_schedule_file,
#                                                            new_wafer_start_schedule)
#     for row in new_schedule_rearranged:
#         print(new_schedule_rearranged[row])
#
#     return new_schedule_rearranged
#     # write_new_wafer_start_schedule(new_wafer_start_schedule)


def get_wafer_start_schedule(wafer_start_schedule_file):

    df_wafer_start_schedule = pd.read_csv(wafer_start_schedule_file)

    wafer_start_schedule = {
                            row: {
                                    'FAB':
                                        df_wafer_start_schedule.loc[row, 'FAB'],
                                    'LOT_ID':
                                        df_wafer_start_schedule.loc[row, 'LOT_ID'],
                                    'LOT_TYPE':
                                        df_wafer_start_schedule.loc[row, 'LOT_TYPE'],
                                    'PRODUCT':
                                        df_wafer_start_schedule.loc[row, 'PRODUCT'],
                                    'START_TIME':
                                        datetime.strptime(
                                            df_wafer_start_schedule.loc[row, 'START_TIME'],
                                            '%d/%m/%Y %H:%M:%S'
                                        ),
                                    'LOT_SIZE':
                                        df_wafer_start_schedule.loc[row, 'LOT_SIZE'],
                                    'DUE_DATE':
                                        datetime.strptime(
                                            df_wafer_start_schedule.loc[row, 'DUE_DATE'],
                                            '%d/%m/%Y %H:%M:%S'
                                        ),
                                    'PRIORITY':
                                        df_wafer_start_schedule.loc[row, 'PRIORITY']}
                                for row in range(df_wafer_start_schedule.shape[0])
                            }

    job_schedule_dict = {
                        wafer_start_schedule[job]['LOT_ID']:
                            {
                                'START_TIME':
                                    wafer_start_schedule[job]['START_TIME'],
                                'DUE_DATE':
                                    wafer_start_schedule[job]['DUE_DATE'],
                                'PRODUCT': wafer_start_schedule[job]['PRODUCT'],
                                'LOT_SIZE': wafer_start_schedule[job]['LOT_SIZE']
                            }
                            for job in wafer_start_schedule
                       }

    products_list = [job_schedule_dict[job]['PRODUCT'] for job in job_schedule_dict.keys()]
    products_set = set(())
    products_set.update(products_list)
    lot_size = {product: [] for product in products_set}
    for job in job_schedule_dict:
        lot_size[job_schedule_dict[job]['PRODUCT']].append(job_schedule_dict[job]['LOT_SIZE'])
    lot_size_max = {product: np.max(lot_size[product]) for product in lot_size.keys()}
    # print('lot size last', lot_size_max)

    return products_set, lot_size_max, df_wafer_start_schedule, job_schedule_dict


def get_scheduling_time_parameter_constraints(start_time_dict):

    start_time_list = [get_release_timestamp(start_time_dict[key]['START_TIME'])
                       for key in start_time_dict.keys()]
    # for start_time in start_time_list:
    #     print('Start time->', start_time_list[start_time]['TIME_STAMP'])
    # start_time_list_sorted = sorted(start_time_list)
    earliest_release_timestamp = np.min(start_time_list)
    # earliest_release_time = datetime.fromtimestamp(earliest_release_timestamp)
    time_gap_list = [
        int((datetime.fromtimestamp(item) - datetime.fromtimestamp(earliest_release_timestamp)) / timedelta(minutes=1))
        for item in start_time_list]
    time_gap_list_nonzero = [item for item in time_gap_list if item > 0]
    # for time_gap_item in time_gap_list_nonzero:
    #     print('Time gap (in minutes) ->', time_gap_item)

    gap_to_due_date_list = [int((start_time_dict[key]['DUE_DATE'] - start_time_dict[key]['START_TIME'])/timedelta(minutes=1))
                            for key in start_time_dict.keys()]

    return earliest_release_timestamp, np.min(time_gap_list_nonzero), np.min(gap_to_due_date_list)


def get_release_timestamp(wafer_start_schedule):

    start_time = int(datetime.timestamp(wafer_start_schedule))

    return start_time


def get_due_date_timestamp(wafer_due_date):

    due_date = datetime.timestamp(wafer_due_date)

    return due_date


def get_new_release_time(wafer_start_time, wafer_due_date, earliest_release, shift_duration_in_hours):

    current_shift = int(np.ceil((wafer_start_time - earliest_release)/shift_duration_in_hours))
    lower_bound = 1
    if current_shift > 1:
        lower_bound = np.min([current_shift - 1, 2])

    due_period = int(np.floor((wafer_due_date - earliest_release)/shift_duration_in_hours))
    upper_bound = 1
    if due_period - current_shift > 1:
        upper_bound = np.min([due_period-current_shift, 2])
    num_list = range(-lower_bound,upper_bound+1)
    rand_num = np.random.randint(0, len(num_list))
    new_start_time = earliest_release + (current_shift + num_list[rand_num])*shift_duration_in_hours
    timestamp_wafer_start_new = get_release_timestamp(new_start_time)
    timestamp_wafer_due_date = get_due_date_timestamp(wafer_due_date)
    new_schedule_gap_to_due_date = int((wafer_due_date - wafer_start_time)/timedelta(minutes=1))
    if np.all([
        timestamp_wafer_start_new >= datetime.timestamp(earliest_release),
        timestamp_wafer_start_new < timestamp_wafer_due_date,
        new_schedule_gap_to_due_date >= 0
        ]):
        return new_start_time
    else:
        return wafer_start_time


def get_new_wafer_start_schedule(wafer_start_schedule_file, new_wafer_start_schedule):

    df_wafer_start_schedule = pd.read_csv(wafer_start_schedule_file)
    wafer_schedule_rearranged = {
                                    row: {
                                            'FAB':
                                                df_wafer_start_schedule.loc[row, 'FAB'],
                                            'LOT_ID':
                                                df_wafer_start_schedule.loc[row, 'LOT_ID'],
                                            'LOT_TYPE':
                                                df_wafer_start_schedule.loc[row, 'LOT_TYPE'],
                                            'PRODUCT':
                                                df_wafer_start_schedule.loc[row, 'PRODUCT'],
                                            'START_TIME':
                                                new_wafer_start_schedule[
                                                    df_wafer_start_schedule.loc[row, 'LOT_ID']],
                                            'LOT_SIZE':
                                                df_wafer_start_schedule.loc[row, 'LOT_SIZE'],
                                            'DUE_DATE':
                                                datetime.strptime(df_wafer_start_schedule.loc[row, 'DUE_DATE'],
                                                                  '%d/%m/%Y %H:%M:%S'),
                                            'PRIORITY':
                                                df_wafer_start_schedule.loc[row, 'PRIORITY']
                                    }
                                    for row in range(df_wafer_start_schedule.shape[0])
                                }

    return wafer_schedule_rearranged


def get_formatted_release_plan(df_wafer_start_schedule, new_wafer_start_schedule):

    # df_wafer_start_schedule = pd.read_csv(wafer_start_schedule_file)
    row = 0
    wafer_schedule_rearranged = dict()
    for product in new_wafer_start_schedule:
        for lot in new_wafer_start_schedule[product]['LOT_DETAILS']:
            wafer_schedule_rearranged[row] = dict()
            wafer_schedule_rearranged[row]['FAB'] = df_wafer_start_schedule.loc[1, 'FAB']
            wafer_schedule_rearranged[row]['LOT_ID'] = lot
            wafer_schedule_rearranged[row]['LOT_TYPE'] = df_wafer_start_schedule.loc[1, 'LOT_TYPE']
            wafer_schedule_rearranged[row]['PRODUCT'] = product
            wafer_schedule_rearranged[row]['START_TIME'] = new_wafer_start_schedule[product]['LOT_DETAILS'][lot]['START_TIME']
            wafer_schedule_rearranged[row]['LOT_SIZE'] = new_wafer_start_schedule[product]['LOT_DETAILS'][lot]['LOT_SIZE']
            wafer_schedule_rearranged[row]['DUE_DATE'] = new_wafer_start_schedule[product]['LOT_DETAILS'][lot]['DUE_DATE']
            wafer_schedule_rearranged[row]['PRIORITY'] = df_wafer_start_schedule.loc[1, 'PRIORITY']
            row += 1

    return wafer_schedule_rearranged


def get_lots_after_resizing(last_wafer_start_schedule, lot_size_proposed,
                            report_start_time, longest_due_date):

    resized_lots = {product: {'TOTAL_WAFERS': [],
                              'NUM_LOTS_ON_RESIZING': 0,
                              'LOT_ID_LIST': [],
                              'LOT_DETAILS': dict()}
                    for product in lot_size_proposed}

    for product in resized_lots:
        for lot in last_wafer_start_schedule.keys():
            if last_wafer_start_schedule[lot]['PRODUCT'] == product:
                resized_lots[product]['TOTAL_WAFERS'].append(
                    last_wafer_start_schedule[lot]['LOT_SIZE'])

    for product in resized_lots:
        resized_lots[product]['NUM_LOTS_ON_RESIZING'] = int(np.ceil(np.sum(resized_lots[product]['TOTAL_WAFERS'])/lot_size_proposed[product]))

    total_lots = 0
    for product in resized_lots:
        total_lots += resized_lots[product]['NUM_LOTS_ON_RESIZING']

    allocated_lots = []
    for product in resized_lots:
        num_wafers_remaining = np.sum(resized_lots[product]['TOTAL_WAFERS'])
        for lot in range(1, total_lots+1):
            # print(product, '->total wafers ->', num_wafers_remaining)
            if len(resized_lots[product]['LOT_ID_LIST']) < resized_lots[product]['NUM_LOTS_ON_RESIZING']:
                if lot not in allocated_lots:
                    size_lot = np.min([lot_size_proposed[product], num_wafers_remaining])
                    resized_lots[product]['LOT_ID_LIST'].append(('LOT_' + str(lot), size_lot))
                    allocated_lots.append(lot)
                    num_wafers_remaining -= np.min([lot_size_proposed[product], num_wafers_remaining])

    for product in resized_lots:
        print(product, '-> number of lots ->', resized_lots[product]['NUM_LOTS_ON_RESIZING'])
        # print('lot ids',  resized_lots[product]['LOT_ID_LIST'])

    for product in resized_lots:
        for lot in resized_lots[product]['LOT_ID_LIST']:
            resized_lots[product]['LOT_DETAILS'][lot[0]] = {
                                                            'START_TIME':
                                                                report_start_time,
                                                            'DUE_DATE':
                                                                longest_due_date,
                                                            'PRODUCT': product,
                                                            'LOT_SIZE': lot[1]
                                                        }

    return resized_lots


def get_tardy_jobs_by_product_from_surrogate_model(product, lot_size_for_product_list):

    df_predicted_performance = pd.read_csv('predicted_performance_at_lot_size.csv')
    for row in range(np.shape(df_predicted_performance)[0]):
        if np.all([
            df_predicted_performance.loc[row, 'PRODUCT'] == product,
            df_predicted_performance.loc[row, 'LOT_SIZE'] == lot_size_for_product_list[product]
            ]):
            return df_predicted_performance.loc[row, 'TARDY_LOTS']


def get_tardy_jobs_by_product(due_delivery, performance_predicted):

    deviation = dict()
    for product in due_delivery:
        deviation[product] = dict()
        for period_ts in due_delivery[product]:
            if due_delivery[product][period_ts] > 0:
                predicted_non_tardy_jobs = [
                                                item[2] for item in performance_predicted
                                                if np.all(
                                                    [item[0] <= period_ts,
                                                    item[1] == get_recipe_for_final_step(product)]
                                                )
                                            ]
                # non_tardy_jobs_from_planning = [ 1
                #                                  for completion_date_ts in lots_completion_info[product]
                #                                     if completion_date_ts <= period_ts
                #                                ]
                deviation[product][period_ts] = {
                    'PREDICTION': np.max([due_delivery[product][period_ts] - np.sum(predicted_non_tardy_jobs), 0])
                    # 'PLANNING': np.max([due_delivery[product][period_ts] - np.sum(non_tardy_jobs_from_planning), 0])
                }
                print('Product: ', product, ' Due date: ', get_due_date_from_timestamp(period_ts),
                          ' target production (lots) ', due_delivery[product][period_ts],
                          ', predicted completion (lots) ', np.sum(predicted_non_tardy_jobs),
                          # ' as per planning ', np.sum(non_tardy_jobs_from_planning),
                          ', deviation ', deviation[product][period_ts]['PREDICTION'])

    return deviation


def get_total_tardy_wafers(num_tardy_jobs, lot_size_proposed):

    total_tardy_wafers = 0
    for product in num_tardy_jobs:
        total_tardy_wafers += np.sum(num_tardy_jobs[product]*lot_size_proposed[product])

    return total_tardy_wafers


# def get_release_time_after_lot_resizing(start_time_dict, earliest_release, possible_num_wafers_per_lot):
#
#     altered_num_wafers_per_lot = possible_num_wafers_per_lot[np.random.randint(1, len(possible_num_wafers_per_lot))]
#     # start_time_list = [get_release_timestamp(start_time_dict[key]['START_TIME'])
#     #                    for key in start_time_dict.keys()]
#     # latest_release = datetime.fromtimestamp(np.max(start_time_list))
#     # num_shifts_in_between = int(np.floor((latest_release - earliest_release)/get_shift_duration()))
#     # possible_release_times = [earliest_release]
#     # for shift in range(1, num_shifts_in_between + 1):
#     #     possible_release_times.append(earliest_release + shift*get_shift_duration())
#
#     total_wafers = [start_time_list[job]['LOT_SIZE'] for job in start_time_dict]
#     int_num_lots = int(np.floor(total_wafers)/altered_num_wafers_per_lot)
#     frac_num_wafers = total_wafers - int_num_lots*altered_num_wafers_per_lot
#
#     new_schedule = dict()
#     for row in range(1, int_num_lots + 1):
#         new_schedule[row] = {
#                                     'FAB':
#                                         start_time_dict.loc[row, 'FAB'],
#                                     'LOT_ID':
#                                         start_time_dict.loc[row, 'LOT_ID'],
#                                     'LOT_TYPE':
#                                         start_time_dict.loc[row, 'LOT_TYPE'],
#                                     'PRODUCT':
#                                         start_time_dict.loc[row, 'PRODUCT'],
#                                     'START_TIME':
#                                         start_time_dict[
#                                             start_time_dict.loc[row, 'LOT_ID']],
#                                     'LOT_SIZE':
#                                         altered_num_wafers_per_lot,
#                                     'DUE_DATE':
#                                         datetime.strptime(df_wafer_start_schedule.loc[row, 'DUE_DATE'],
#                                                           '%d/%m/%Y %H:%M:%S'),
#                                     'PRIORITY':
#                                         df_wafer_start_schedule.loc[row, 'PRIORITY']
#                             }


# def get_wafers_completion_prediction_under_recipe(upto_period, start_period, performance_file):
#
#     performance_file = os.getcwd() + '/' + performance_file
#
#     df_completion_performance = pd.read_csv(performance_file)
#     print('Read predicted performance file')
#     recipe_list = [df_completion_performance.loc[row, 'RecipeName']
#                         for row in range(df_completion_performance.shape[0])
#                             if df_completion_performance.loc[row, 'PeriodNumber'] <= upto_period]
#     # for item in recipe_list:
#     #     print('Recipe (list) ->', item)
#     recipe_set = set(())
#     recipe_set.update(recipe_list)
#     # print('Items in recipe set ->', len(recipe_set))
#
#     completion_dict = dict()
#     for period in range(1, upto_period + 1):
#         print('Checking period', period)
#         completion_dict[period] = dict()
#         for recipe in recipe_set:
#             # print('Period->', period, '->Recipe->', recipe)
#             completion_dict[period][recipe] = 0.0
#             for row in range(df_completion_performance.shape[0]):
#                 if df_completion_performance.loc[row, 'PeriodNumber'] <= upto_period:
#                     if np.all([
#                         period == df_completion_performance.loc[row, 'PeriodNumber'] + start_period,
#                         recipe == df_completion_performance.loc[row, 'RecipeName']
#                     ]):
#                         # print('Prediction time scale', df_completion_performance.loc[row, 'PeriodNumber'],
#                         #       'shifted to ', period)
#                         completion_dict[period][recipe] = df_completion_performance.loc[row, 'CompletedLots']
#
#     # for period in completion_dict.keys():
#     #     print('period->', period, '->', completion_dict[period])
#
#     cumulative_completion = dict()
#     for period in completion_dict:
#         cumulative_completion[period] = dict()
#         if period > 1:
#             # print('Period->', period, '->num recipes->', len(completion_dict[period].keys()))
#             for recipe in completion_dict[period]:
#                 # print('Cumulative completion: Period->', period, '->Recipe->', recipe)
#                 cumulative_completion[period][recipe] = cumulative_completion[period - 1][recipe] + \
#                                                             completion_dict[period][recipe]
#         else:
#             # print('Period->', period, '->num recipes->', len(completion_dict[period].keys()))
#             cumulative_completion[period] = completion_dict[period]
#             # for recipe in cumulative_completion[period].keys():
#             #     print('Cumulative completion: Period->', period, '->recipe->', recipe)
#
#     return cumulative_completion


def get_wafers_completion_prediction_under_recipe(upto_due_date_ts, start_period, performance_file):

    performance_file = os.getcwd() + '/' + performance_file
    products = ['PRO_14', 'PRO_15']
    recipes_in_final_steps = [get_recipe_for_final_step(product) for product in products]
    df_completion_performance = pd.read_csv(performance_file)
    # print('Read predicted performance file')
    period_end_time_list = []
    for row in range(df_completion_performance.shape[0]):
        period_end_time_ts = datetime.timestamp(datetime.strptime(
                df_completion_performance.loc[row, 'PeriodEndTime'],
                '%d/%m/%Y %H:%M:%S'))
        if np.all([period_end_time_ts <= upto_due_date_ts,
                   df_completion_performance.loc[row, 'RecipeName'] in recipes_in_final_steps]):
            period_end_time_list.append(
                (
                    int(period_end_time_ts),
                    df_completion_performance.loc[row, 'RecipeName'],
                    df_completion_performance.loc[row, 'CompletedLots']
                ))

    # completion_dict = dict()
    # for period_end_time in period_end_time_list:
    #     print('Checking period', period_end_time)
    #     completion_dict[period_end_time] = dict()
    #     for recipe in recipe_set:
    #         # print('Period->', period, '->Recipe->', recipe)
    #         completion_dict[period_end_time][recipe] = 0.0
    #         for row in range(df_completion_performance.shape[0]):
    #             if df_completion_performance.loc[row, 'PeriodNumber'] <= upto_period:
    #                 if np.all([
    #                     period == df_completion_performance.loc[row, 'PeriodNumber'] + start_period,
    #                     recipe == df_completion_performance.loc[row, 'RecipeName']
    #                 ]):
    #                     # print('Prediction time scale', df_completion_performance.loc[row, 'PeriodNumber'],
    #                     #       'shifted to ', period)
    #                     completion_dict[period][recipe] = df_completion_performance.loc[row, 'CompletedLots']
    #
    # # for period in completion_dict.keys():
    # #     print('period->', period, '->', completion_dict[period])
    #
    # cumulative_completion = dict()
    # for period in completion_dict:
    #     cumulative_completion[period] = dict()
    #     if period > 1:
    #         # print('Period->', period, '->num recipes->', len(completion_dict[period].keys()))
    #         for recipe in completion_dict[period]:
    #             # print('Cumulative completion: Period->', period, '->Recipe->', recipe)
    #             cumulative_completion[period][recipe] = cumulative_completion[period - 1][recipe] + \
    #                                                         completion_dict[period][recipe]
    #     else:
    #         # print('Period->', period, '->num recipes->', len(completion_dict[period].keys()))
    #         cumulative_completion[period] = completion_dict[period]
    #         # for recipe in cumulative_completion[period].keys():
    #         #     print('Cumulative completion: Period->', period, '->recipe->', recipe)

    # return cumulative_completion
    return period_end_time_list


def get_wafers_completed_under_recipe(upto_due_date_ts):

    performance_file = os.getcwd() + get_target_dir() + '/output/stat_eqptgroup_recipe.csv'

    df_completion_performance = pd.read_csv(performance_file)
    products = ['PRO_14', 'PRO_15']
    recipes_in_final_steps = [get_recipe_for_final_step(product) for product in products]
    # df_completion_performance = pd.read_csv(performance_file)
    # print('Read predicted performance file')
    period_end_time_list = []
    # print('shape', df_completion_performance.shape[0])
    for row in range(df_completion_performance.shape[0]):
        period_end_time_ts = datetime.timestamp(datetime.strptime(
                df_completion_performance.loc[row, 'PeriodEndTime'],
                '%d/%m/%Y %H:%M:%S'))
        # print('period ends', period_end_time_ts)
        if np.all([period_end_time_ts <= upto_due_date_ts,
                   df_completion_performance.loc[row, 'RecipeName'] in recipes_in_final_steps]):
            period_end_time_list.append(
                (
                    int(period_end_time_ts),
                    df_completion_performance.loc[row, 'RecipeName'],
                    df_completion_performance.loc[row, 'CompletedLots']
                ))
    # print('Lots completed', period_end_time_list)
    return period_end_time_list


def get_predicted_performance(predicted_performance_file, upto_period):

    df_predicted_performance = pd.read_csv(predicted_performance_file)
    predicted_performance = {
                                df_predicted_performance.loc[row, 'PeriodNumber']:
                                 {
                                     df_predicted_performance.loc[row, 'RecipeName']:
                                      df_predicted_performance.loc[row, 'CompletedLots']
                                 }
                                for row in range(df_predicted_performance.shape[0])
                                    if df_predicted_performance.loc[row, 'PeriodNumber'] <= upto_period
                             }

    predicted_performance_cumulative = dict()
    for period in predicted_performance_cumulative:
        if period <= 1:
            predicted_performance_cumulative[period] = predicted_performance[period]
        else:
            for recipe in predicted_performance[period]:
                predicted_performance_cumulative[period][recipe] = predicted_performance_cumulative[period-1][recipe] + predicted_performance[period][recipe]

    return predicted_performance_cumulative


# def get_due_delivery(wafer_start_schedule, start_datetime, upto_shift):
#
#     # wafer_start_schedule = get_wafer_start_schedule(wafer_schedule_file)
#     start_time_list = [get_release_timestamp(wafer_start_schedule[key]['START_TIME'])
#                        for key in wafer_start_schedule.keys()]
#     # for start_time in start_time_list:
#     #     print('Start time->', start_time_list[start_time]['TIME_STAMP'])
#     # start_time_list_sorted = sorted(start_time_list)
#     due_dates_for_products = dict()
#     # earliest_release_index = np.min(start_time_list)
#     # start_datetime = datetime.fromtimestamp(earliest_release_index)
#     eligible_lots = []
#     for lot in wafer_start_schedule:
#         due_date_period = np.int(
#                 np.ceil((wafer_start_schedule[lot]['DUE_DATE'] - start_datetime)/get_shift_duration())
#         )
#         # print('Due date period ->', due_date_period, '<-', upto_shift)
#         if due_date_period <= upto_shift:
#             # print(lot, '->', wafer_start_schedule[lot]['PRODUCT'])
#             # print('Due date period ->', due_date_period, '<-', upto_shift)
#             product = wafer_start_schedule[lot]['PRODUCT']
#             # print('Product', product)
#             if product in due_dates_for_products.keys():
#                 due_dates_for_products[product].append(wafer_start_schedule[lot]['DUE_DATE'])
#             else:
#                 due_dates_for_products[product] = []
#                 due_dates_for_products[product].append(wafer_start_schedule[lot]['DUE_DATE'])
#     # print('Product list for deliverables->', due_dates_for_products.keys())
#
#     delivery_quant_at_period = dict()
#     for product in due_dates_for_products.keys():
#         delivery_date_ts_list = [int(datetime.timestamp(date_time))
#                                  for date_time in due_dates_for_products[product]]
#         delivery_date_set = set(())
#         delivery_date_set.update(delivery_date_ts_list)
#         delivery_quant_at_period[product] = dict()
#         for due_date in delivery_date_set:
#             period = get_period_from_datetime(due_date, start_datetime)
#             # print('Due date ->', due_date, '->', period)
#             delivery_quant_at_period[product][period] = []
#         for due_date in delivery_date_ts_list:
#             period = get_period_from_datetime(due_date, start_datetime)
#             delivery_quant_at_period[product][period].append(1)
#     # print('Deliverables->', delivery_quant_at_period)
#
#     delivery_quant_cumulative = dict()
#     for product in delivery_quant_at_period:
#         delivery_quant_cumulative[product] = dict()
#         delivery_periods = [key for key in delivery_quant_at_period[product].keys()]
#         longest_delivery_period = np.max(delivery_periods)
#         for period in range(1, longest_delivery_period+1):
#             if period == 1:
#                 if period in delivery_quant_at_period[product].keys():
#                     delivery_quant_cumulative[product][period] = np.sum(delivery_quant_at_period[product][period])
#                 else:
#                     delivery_quant_cumulative[product][period] = 0
#             else:
#                 if period in delivery_quant_at_period[product].keys():
#                     delivery_quant_cumulative[product][period] = delivery_quant_cumulative[product][period-1] + \
#                                                                 np.sum(delivery_quant_at_period[product][period])
#                 else:
#                     delivery_quant_cumulative[product][period] = delivery_quant_cumulative[product][period-1]
#
#     # for product in delivery_quant_cumulative:
#     #     for period in delivery_quant_cumulative[product]:
#     #         print('Product->', product, '->period->', period,
#     #               '->quant->', delivery_quant_cumulative[product][period])
#
#     return delivery_quant_cumulative


def get_due_delivery(wafer_start_schedule, start_datetime, upto_due_date):

    # wafer_start_schedule = get_wafer_start_schedule(wafer_schedule_file)
    start_time_list = [get_release_timestamp(wafer_start_schedule[key]['START_TIME'])
                       for key in wafer_start_schedule.keys()]
    # for start_time in start_time_list:
    #     print('Start time->', start_time_list[start_time]['TIME_STAMP'])
    # start_time_list_sorted = sorted(start_time_list)
    due_dates_for_products = dict()
    # earliest_release_index = np.min(start_time_list)
    # start_datetime = datetime.fromtimestamp(earliest_release_index)
    eligible_lots = []
    for lot in wafer_start_schedule:
        # due_date_period = np.int(
        #         np.ceil((wafer_start_schedule[lot]['DUE_DATE'] - start_datetime)/get_shift_duration())
        # )
        # print('Due date period ->', due_date_period, '<-', upto_shift)
        if datetime.timestamp(wafer_start_schedule[lot]['DUE_DATE']) <= upto_due_date:
            # print(lot, '->', wafer_start_schedule[lot]['PRODUCT'])
            # print('Due date period ->', due_date_period, '<-', upto_shift)
            product = wafer_start_schedule[lot]['PRODUCT']
            # print('Product', product)
            if product in due_dates_for_products.keys():
                due_dates_for_products[product].append(wafer_start_schedule[lot]['DUE_DATE'])
            else:
                due_dates_for_products[product] = []
                due_dates_for_products[product].append(wafer_start_schedule[lot]['DUE_DATE'])
    # print('Product list for deliverables->', due_dates_for_products.keys())

    delivery_quant = dict()
    for product in due_dates_for_products.keys():
        delivery_date_ts_list = [int(datetime.timestamp(date_time))
                                 for date_time in due_dates_for_products[product]]
        delivery_date_set = set(())
        delivery_date_set.update(delivery_date_ts_list)
        delivery_quant[product] = dict()
        for due_date in delivery_date_set:
            # period = get_period_from_datetime(due_date, start_datetime)
            # print('Due date ->', due_date, '->', period)
            delivery_quant[product][due_date] = []
        for due_date in delivery_quant[product]:
            for delivery_date_time in delivery_date_ts_list:
                if delivery_date_time <= due_date:
                    delivery_quant[product][due_date].append(1)

    delivery_quant_cumulative = dict()
    for product in delivery_quant:
        delivery_quant_cumulative[product] = dict()
        for due_date in delivery_quant[product]:
            delivery_quant_cumulative[product][due_date] = np.sum(delivery_quant[product][due_date])

    # for product in delivery_quant_cumulative:
    #     for period in delivery_quant_cumulative[product]:
    #         print('Product->', product, '->period->', period,
    #               '->quant->', delivery_quant_cumulative[product][period])

    return delivery_quant_cumulative


def get_period_from_datetime(due_date, start_datetime):

    due_period = np.int(np.ceil((datetime.fromtimestamp(due_date) - start_datetime)/get_shift_duration()))

    return due_period


def get_shift_duration():

    df_model_config = pd.read_csv(os.getcwd() + get_target_dir() + get_model_config_file())
    for row in range(df_model_config.shape[0]):
        if df_model_config.loc[row, 'PARAMETER_NAME'] == 'REPORT_INTERVAL_HOUR':
            shift_duration_hours = timedelta(hours=int(df_model_config.loc[row, 'PARAMETER_VALUE']))

            return shift_duration_hours


def get_recipe_for_final_step(product):

    recipe_for_final_step = {
                                'PRO_14': 'REC_LOT_111',
                                'PRO_15': 'REC_LOT_111_1'
                            }

    return recipe_for_final_step[product]


def get_longest_due_date(wafer_start_schedule):

    due_date_timestamp_list = [get_due_date_timestamp(wafer_start_schedule[item]['DUE_DATE'])
                               for item in wafer_start_schedule]
    longest_due_date_timestamp = np.max(due_date_timestamp_list)
    longest_due_date_datetime = datetime.fromtimestamp(longest_due_date_timestamp)

    return longest_due_date_datetime, longest_due_date_timestamp


def get_earliest_release_time(wafer_start_schedule):

    start_time_list = [get_release_timestamp(wafer_start_schedule[key]['START_TIME'])
                       for key in wafer_start_schedule.keys()]
    # for start_time in start_time_list:
    #     print('Start time->', start_time_list[start_time]['TIME_STAMP'])
    # start_time_list_sorted = sorted(start_time_list)
    earliest_release_timestamp = np.min(start_time_list)
    earliest_release_datetime = datetime.fromtimestamp(earliest_release_timestamp)

    return earliest_release_datetime


def get_start_period(wafer_start_schedule, report_start_datetime):

    start_datetime = get_earliest_release_time(wafer_start_schedule)
    # print('Earliest release at', start_datetime)
    start_at_period = int(np.ceil((start_datetime + timedelta(hours=0) - report_start_datetime)/get_shift_duration()))

    return start_at_period


def get_report_start_time(wafer_start_schedule):

    start_datetime = get_earliest_release_time(wafer_start_schedule)
    report_start_datetime = datetime(year=start_datetime.year,
                                     month=start_datetime.month,
                                     day=start_datetime.day,
                                     hour=0,
                                     minute=0,
                                     second=0)

    # model_config_file = os.getcwd() + '/input/model_config.csv'
    # df_model_config = pd.read_csv(model_config_file)
    #
    # for row in range(df_model_config.shape[0]):
    #     if df_model_config.loc[row, 'PARAMETER_NAME'] == 'REPORT_START_TIME':
    #         report_start_time = datetime.strptime(df_model_config.loc[row, 'PARAMETER_VALUE'],
    #                                               '%d/%m/%Y %H:%M:%S')
    #         return report_start_time

    return report_start_datetime


def update_model_configuration(report_start_datetime):

    # print('Updating model configuration')
    model_config = os.getcwd() + get_target_dir() + get_model_config_file()
    df_model_config = pd.read_csv(model_config)
    updated_model = dict()
    # # timing_details = dict()
    # print('Model->', df_model_config)
    for row in range(df_model_config.shape[0]):
        updated_model[row] = dict()
        updated_model[row]['PARAMETER_NAME'] = df_model_config.loc[row, 'PARAMETER_NAME']
        updated_model[row]['PARAMETER_VALUE'] = df_model_config.loc[row, 'PARAMETER_VALUE']
        if df_model_config.loc[row, 'PARAMETER_NAME'] == 'REPORT_START_TIME':
            # print('Report start date', report_start_datetime)
            updated_model[row]['PARAMETER_VALUE'] = datetime.strftime(report_start_datetime, '%d/%m/%Y %H:%M:%S')
        if df_model_config.loc[row, 'PARAMETER_NAME'] == 'START_TIME':
            start_datetime = report_start_datetime - timedelta(minutes=21)
            # print('Start date', start_datetime)
            updated_model[row]['PARAMETER_VALUE'] = datetime.strftime(start_datetime, '%d/%m/%Y %H:%M:%S')
        if df_model_config.loc[row, 'PARAMETER_NAME'] == 'START_PRINT_TRACE':
            start_print_datetime = report_start_datetime - timedelta(minutes=21)
            # print('Start date', start_print_datetime)
            updated_model[row]['PARAMETER_VALUE'] = datetime.strftime(start_print_datetime, '%d/%m/%Y %H:%M:%S')
        if df_model_config.loc[row, 'PARAMETER_NAME'] == 'END_TIME':
            end_datetime = report_start_datetime + timedelta(days=7)
            # print('End date', end_datetime)
            updated_model[row]['PARAMETER_VALUE'] = datetime.strftime(end_datetime, '%d/%m/%Y %H:%M:%S')
        if df_model_config.loc[row, 'PARAMETER_NAME'] == 'END_PRINT_TRACE':
            end_print_datetime = report_start_datetime + timedelta(days=7)
            # print('End date', end_print_datetime)
            updated_model[row]['PARAMETER_VALUE'] = datetime.strftime(end_print_datetime, '%d/%m/%Y %H:%M:%S')

    # print('Updated model', updated_model)
    print('Updated model')

    return updated_model


def get_gap_in_shifts(from_date_time, to_date_time, shift_duration_hrs):

    num_shifts = int((to_date_time - from_date_time)/shift_duration_hrs)

    return num_shifts


def suggest_strategies(planned_wafer_start_schedule, report_start_time, longest_due_date, shift_duration_hrs):

    average_cycle_time = {'PRO_14': 3.5, 'PRO_15': 5.5}
    gap_release_to_delivery_dates = {
                                        lot: get_gap_in_shifts(
                                        planned_wafer_start_schedule[lot]['START_TIME'],
                                        planned_wafer_start_schedule[lot]['DUE_DATE'],
                                        shift_duration_hrs)
                                        for lot in planned_wafer_start_schedule
                                    }

    gap_start_to_delivery_dates = {
                                        lot: get_gap_in_shifts(
                                        report_start_time,
                                        planned_wafer_start_schedule[lot]['DUE_DATE'],
                                        shift_duration_hrs)
                                        for lot in planned_wafer_start_schedule
                                    }
    possibility_to_pushback_release_dates = dict()
    possibility_to_reduce_tardiness_by_adjusting_release_timing = dict()
    for lot in planned_wafer_start_schedule:
        possibility_to_pushback_release_dates[lot] = gap_start_to_delivery_dates[lot] - gap_release_to_delivery_dates[lot]
        if possibility_to_pushback_release_dates[lot] > 0:
            product = planned_wafer_start_schedule[lot]['PRODUCT']
            possibility_to_reduce_tardiness_by_adjusting_release_timing[lot] = gap_start_to_delivery_dates[lot] - average_cycle_time[product]
        else:
            possibility_to_reduce_tardiness_by_adjusting_release_timing[lot] = 0

    # possibility_to_pushback_release_dates_list = [1 for lot in possibility_to_pushback_release_dates
    #                                               if possibility_to_pushback_release_dates[lot] > 0]

    possibility_to_reduce_tardiness_by_adjusting_release_timing_list = [1 for lot in possibility_to_reduce_tardiness_by_adjusting_release_timing
                                                                        if possibility_to_reduce_tardiness_by_adjusting_release_timing[lot] > 0]

    if np.sum(possibility_to_reduce_tardiness_by_adjusting_release_timing_list) > int(0.2*len(possibility_to_reduce_tardiness_by_adjusting_release_timing)):
        print('Try release timing adjustment and change in lot sizes.')
        return ['release_date', 'lot_size_change']
    else:
        print('Not possible to reduce tardiness by adjusting release timings as release dates of most wafers',
              'are already close to the earliest possible release time.')
        print('Try change in lot sizes.')
        return ['lot_size_change']


def get_production_periods(due_date_upto, report_start_time, shift_duration_in_hours):

    num_periods = np.int(np.ceil((due_date_upto - report_start_time)/shift_duration_in_hours))

    # print('Num periods', num_periods)
    return num_periods


def find_best_suggestion(num_tardy_jobs, num_tardy_jobs_predicted):

    best_iteration = [0]
    min_tardy_jobs = num_tardy_jobs_predicted
    print('Number of tardy wafers ->', num_tardy_jobs)
    for iteration in num_tardy_jobs:
        if num_tardy_jobs[iteration] < min_tardy_jobs:
            min_tardy_jobs = num_tardy_jobs[iteration]
            best_iteration.pop()
            best_iteration.append(iteration)

    print('Best suggestion at iteration: ', best_iteration[0])

    return best_iteration[0]


def compare_planned_vs_predicted_lateness(planned_wafer_start_schedule, start_datetime,
                                                                    predicted_performance_file):

    due_delivery = dict()
    # earliest_release_index = np.min(start_time_list)
    # start_datetime = datetime.fromtimestamp(earliest_release_index)
    period_duration = timedelta(hours=24)
    eligible_lots = []
    for lot in planned_wafer_start_schedule:
        product = planned_wafer_start_schedule[lot]['PRODUCT']
        if product in due_delivery.keys():
            days_apart = int(np.ceil((planned_wafer_start_schedule[lot]['DUE_DATE'] - start_datetime) / period_duration))
            due_delivery[product].append(days_apart)
        else:
            days_apart = int(np.ceil((planned_wafer_start_schedule[lot]['DUE_DATE'] - start_datetime) / period_duration))
            due_delivery[product] = []
            due_delivery[product].append(days_apart)
    # print('Product list for deliverables->', due_dates_for_products.keys())

    delivery_due_at_day = dict()
    for product in due_delivery.keys():
        delivery_date_difference_list = [days_apart for days_apart in due_delivery[product]]
        delivery_date_set = set(())
        delivery_date_set.update(delivery_date_difference_list)
        delivery_due_at_day[product] = dict()
        for due_date in delivery_date_set:
            delivery_due_at_day[product][due_date] = []
        for due_date in delivery_date_set:
            for other_date in delivery_date_difference_list:
                if int(other_date) <= int(due_date):
                    delivery_due_at_day[product][due_date].append(1)

    cumulative_delivery_quant = dict()
    for product in delivery_due_at_day.keys():
        cumulative_delivery_quant[product] = dict()
        for due_date in delivery_due_at_day[product].keys():
            cumulative_delivery_quant[product][due_date] = np.sum(delivery_due_at_day[product][due_date])

    print('Cumulative deliverables', cumulative_delivery_quant)

    performance_file = os.getcwd() + '/' + predicted_performance_file

    df_completion_performance = pd.read_csv(performance_file)
    print('Read predicted performance file')
    recipe_list = [df_completion_performance.loc[row, 'RecipeName']
                        for row in range(df_completion_performance.shape[0])]
    # for item in recipe_list:
    #     print('Recipe (list) ->', item)
    recipe_set = set(())
    recipe_set.update(recipe_list)
    # print('Items in recipe set ->', len(recipe_set))

    completion_dict = dict()
    for product in cumulative_delivery_quant.keys():
        completion_dict[product] = dict()
        recipe_at_last_step = get_recipe_for_final_step(product)
        for due_day in cumulative_delivery_quant[product].keys():
            # print('Period->', period, '->Recipe->', recipe)
            completion_dict[product][due_day] = []
            for row in range(df_completion_performance.shape[0]):
                if np.all([
                            int(np.ceil((datetime.strptime(df_completion_performance.loc[row, 'PeriodEndTime'], '%d/%m/%Y %H:%M:%S')
                                           - start_datetime)/timedelta(hours=24))) <= due_day,
                            recipe_at_last_step == df_completion_performance.loc[row, 'RecipeName']
                        ]):
                    # print('Prediction time scale', df_completion_performance.loc[row, 'PeriodNumber'],
                    #           'shifted to ', period)
                    completion_dict[product][due_day].append(df_completion_performance.loc[row, 'CompletedLots'])
    print('Completion details (within due days)', completion_dict)

    cumulative_completion = dict()
    for product in completion_dict.keys():
        cumulative_completion[product] = dict()
        for due_day in completion_dict[product].keys():
            cumulative_completion[product][due_day] = np.sum(completion_dict[product][due_day])
    print('Completion details (cumulative figure)', cumulative_completion)

    planned_vs_predicted_delivery = dict()
    for product in cumulative_delivery_quant.keys():
        planned_vs_predicted_delivery[product] = dict()
        for due_day in cumulative_delivery_quant[product].keys():
            planned_vs_predicted_delivery[product][due_day] = {
                                                                'PLANNED': cumulative_delivery_quant[product][due_day],
                                                                'PREDICTED': cumulative_completion[product][due_day]
                                                               }

    return planned_vs_predicted_delivery


def get_completion_info_in_planned_performance(plan_file):

    df_plan = pd.read_csv(plan_file)
    # products_list = [df_plan.loc[row, 'textBox29'] for row in range(np.shape(df_plan)[0])]
    products = {'PRO_14': 41, 'PRO_15': 50}
    products.update(products)
    lots_completion = {product: [] for product in products}
    for row in range(np.shape(df_plan)[0]):
        product = df_plan.loc[row, 'textBox29']
        if products[product] == df_plan.loc[row, 'textBox21']:
            completion_date = datetime.timestamp(datetime.strptime(df_plan.loc[row, 'textBox16'], '%d/%m/%Y %H:%M:%S'))
            lots_completion[product].append(int(completion_date))

    # print('Completion TS lots:', lots_completion)
    return lots_completion


def get_due_date_from_timestamp(due_date_ts):

    due_date = datetime.fromtimestamp(due_date_ts)

    return due_date


def get_current_date_time():

    date_time_now = datetime.now()

    return date_time_now

def get_time_difference(from_time_instant, to_time_instant):

    seconds_elapsed = int((to_time_instant - from_time_instant)/timedelta(seconds=1))

    return seconds_elapsed
