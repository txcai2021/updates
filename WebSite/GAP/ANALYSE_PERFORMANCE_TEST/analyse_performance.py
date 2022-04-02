import numpy as np
import os
import sys
import csv

from func_interface import get_wafer_start_schedule_file
from func_interface import get_target_dir
from func_interface import write_new_configuration
from func_interface import write_new_wafer_start_schedule
from func_interface import get_last_result
from func_interface import write_planned_vs_obtained_performance_in_lateness

from func_interface import write_wafer_start_schedule_from_file
from func_interface import write_improved_wafer_start_schedule
from func_interface import write_recommended_lot_size


# from func_performance_analysis import rearrange_wafer_start_schedule
from func_performance_analysis import get_current_date_time
from func_performance_analysis import suggest_strategies
from func_performance_analysis import get_wafer_start_schedule
from func_performance_analysis import get_longest_due_date
from func_performance_analysis import get_due_date_from_timestamp
from func_performance_analysis import get_wafers_completed_under_recipe
from func_performance_analysis import get_wafers_completion_prediction_under_recipe
from func_performance_analysis import get_report_start_time
from func_performance_analysis import get_shift_duration
from func_performance_analysis import get_start_period
from func_performance_analysis import get_production_periods
from func_performance_analysis import get_recipe_for_final_step
from func_performance_analysis import get_formatted_release_plan
from func_performance_analysis import get_new_release_time
from func_performance_analysis import get_tardy_jobs_by_product
from func_performance_analysis import get_tardy_jobs_by_product_from_surrogate_model
from func_performance_analysis import get_total_tardy_wafers
from func_performance_analysis import get_new_wafer_start_schedule
from func_performance_analysis import find_best_suggestion
from func_performance_analysis import get_predicted_performance
from func_performance_analysis import get_completion_info_in_planned_performance
# from func_performance_analysis import get_average_machine_utilization
from func_performance_analysis import get_due_delivery
from func_performance_analysis import get_lots_after_resizing
from func_performance_analysis import update_model_configuration
from func_performance_analysis import compare_planned_vs_predicted_lateness
from func_run_simulator import run_simulator
from func_performance_analysis import get_time_difference

# ---------------------------------------------------------------------------------------------------------------
simulation_start_time = get_current_date_time()
planned_wafer_start_schedule_file = sys.argv[1]
# plan_details = sys.argv[2]
# lots_completion_info = get_completion_info_in_planned_performance(plan_details)
# print('As per plan:')
# for product in lots_completion_info:
#     print('product', product, '\n')
#     for item in lots_completion_info[product]:
#         print(get_due_date_from_timestamp(item))
predicted_performance_file = sys.argv[2]
# outer_loop_iteration = sys.argv[3]
products_set, lot_size, df_start_schedule, planned_wafer_start_schedule = get_wafer_start_schedule(planned_wafer_start_schedule_file)
print('--------------------------------\n',
      'RESULTS EVALUATED FOR LAST PLAN\n',
      '--------------------------------')
print('Lot size used', lot_size)

report_start_time = get_report_start_time(planned_wafer_start_schedule)
# print('Report start time', report_start_time)
# updated_model_configuration = update_model_configuration(report_start_time)
# write_new_configuration(updated_model_configuration)
longest_due_date, longest_due_date_ts = get_longest_due_date(planned_wafer_start_schedule)
# print('Due date upto', longest_due_date, '->', longest_due_date_ts)
shift_duration_in_hours = get_shift_duration()
start_period = get_start_period(planned_wafer_start_schedule, report_start_time)
# print('First release at ', start_period)
production_periods = get_production_periods(longest_due_date, report_start_time, shift_duration_in_hours)
# # write_wafer_start_schedule_from_file(planned_wafer_start_schedule_file)
# # run_simulator(os.getcwd() + get_target_dir())
due_delivery = get_due_delivery(planned_wafer_start_schedule, report_start_time, longest_due_date_ts)
# print('Delivery due:')
# for product in due_delivery:
#     print('Product', product, ', due date ')

performance_predicted = get_wafers_completion_prediction_under_recipe(longest_due_date_ts,
                                                                    start_period,
                                                                    predicted_performance_file)
# print('Predicted performance', performance_predicted)

suggested_start_schedules = dict()
# suggested_start_schedules[0] = df_start_schedule
#
# # for product in due_delivery:
# #     for period in due_delivery[product]:
# #         print('Product: ', product, ' Period: ', period,
# #               ' Target: ', due_delivery[product][period],
# #               ' Obtained: ', performance_at_trial[period][get_recipe_for_final_step(product)])
# #
deviation = dict()
deviation[0] = get_tardy_jobs_by_product(due_delivery, performance_predicted)

num_tardy_jobs_predicted = {product: [] for product in deviation[0].keys()}
for product in num_tardy_jobs_predicted:
    num_tardy_jobs_predicted[product] = [deviation[0][product][period_ts]['PREDICTION']
                                         for period_ts in deviation[0][product]]

print('Total number of tardy wafers ->', get_total_tardy_wafers(num_tardy_jobs_predicted, lot_size))
print('-------------------')
print('SUGGESTING NEW PLAN')
print('-------------------')
suggested_strategies = suggest_strategies(planned_wafer_start_schedule,
                                          report_start_time,
                                          longest_due_date,
                                          shift_duration_in_hours)

if 'lot_size_change' in suggested_strategies:
    # max_tardy_product = ['PRO_14']
    # max_tardiness = 0
    # for product in num_tardy_jobs_predicted:
    #     print(product, '->', np.sum(num_tardy_jobs_predicted[product])*lot_size[product])
    #     tardy_jobs = get_total_tardy_wafers(num_tardy_jobs_predicted, lot_size)
    #     print('tardy', tardy_jobs)
    #     if tardy_jobs > max_tardiness:
    #         max_tardiness = np.sum(num_tardy_jobs_predicted[product])*lot_size[product]
    #         max_tardy_product.pop()
    #         max_tardy_product.append(product)
    # print('Max tardy product', max_tardy_product[0])

    min_lot_size = 10
    lot_sizes_list = [25, 20, 15, 10]
    lot_size_already_used = [lot_size[product] for product in lot_size]
    lot_sizes_to_try = [item for item in lot_sizes_list if item not in lot_size_already_used]
    print('Lot size to try', lot_sizes_to_try)

    num_suggestions = len(lot_sizes_to_try)
    num_tardy_jobs = dict()
    total_tardy_wafers = dict()
    lot_size_by_iteration = dict()
    for iteration in range(1, num_suggestions + 1):
        print('---------')
        print('ITERATION', iteration)
        print('---------')
        lot_size_proposed = {product: lot_size[product] for product in lot_size}

        for product in lot_size:
            # if lot_size[product] - change_in_lot_size*iteration >= min_lot_size:
                lot_size_proposed[product] = lot_sizes_to_try[iteration-1]
        # print('lot_size', lot_size)
        print('Lot size suggested in iteration', iteration, '->', lot_size_proposed)
        lot_size_by_iteration[iteration] = lot_size_proposed

        # lots_after_resize = get_lots_after_resizing(planned_wafer_start_schedule,
        #                                         lot_size_proposed,
        #                                         report_start_time,
        #                                         longest_due_date)
        #
        # formatted_release_plan = get_formatted_release_plan(df_start_schedule, lots_after_resize)
        #
        # suggested_start_schedules[iteration] = formatted_release_plan
        # write_new_wafer_start_schedule(suggested_start_schedules[iteration])
        # run_simulator(os.getcwd() + get_target_dir())
        # performance_new_schedule = get_wafers_completed_under_recipe(longest_due_date_ts)
        # due_delivery_new = get_due_delivery(suggested_start_schedules[iteration],
        #                                     report_start_time, longest_due_date_ts)

        # deviation[iteration] = get_tardy_jobs_by_product(due_delivery_new, performance_new_schedule)

        # print('new schedule', iteration, ' deviation ', deviation[iteration])
        num_tardy_jobs[iteration] = {product: [] for product in num_tardy_jobs_predicted.keys()}
        for product in num_tardy_jobs[iteration]:
            num_tardy_jobs[iteration][product] = get_tardy_jobs_by_product_from_surrogate_model(product, lot_size_by_iteration[iteration])
        total_tardy_wafers[iteration] = get_total_tardy_wafers(num_tardy_jobs[iteration], lot_size_proposed)
        print('Iteration', iteration, '-> total tardiness (wafers) ->', total_tardy_wafers[iteration])

    print('---------')
    print('NEW PLAN')
    print('---------')

    best_iteration = find_best_suggestion(
                                            total_tardy_wafers,
                                            get_total_tardy_wafers(
                                            num_tardy_jobs_predicted,
                                            lot_size)
                                         )
#     # # best_iteration = 0
    print('Proposed lot size:', lot_size_by_iteration[best_iteration])
    if best_iteration > 0:
#         write_improved_wafer_start_schedule(suggested_start_schedules[best_iteration],
#                                             'IMPROVED_WAFER_START_SCHEDULE')
        write_recommended_lot_size(lot_size_by_iteration[best_iteration],
                                   'IMPROVED_WAFER_START_SCHEDULE')
        # print('New release plan created')
    else:
        print('No improvement obtained by changing lot sizes')
        # write_improved_wafer_start_schedule(suggested_start_schedules[best_iteration], 'IMPROVED_WAFER_START_SCHEDULE')
# else:
#     print('Functions to change release time has been commented out')
#
# print('Run time:', get_time_difference(simulation_start_time, get_current_date_time()), 'seconds')