model_file = open("interaction_model.txt", "r")
model_lines = model_file.readlines()
model_file.close()

time_file = open("time_conditions.txt", "r")
time_lines = time_file.readlines()
time_file.close()

new_lines = []

the_bitch = '{tempo}'
i = 0
for model_line in model_lines:
	if the_bitch not in model_line:
		i += 1
		new_lines.append(model_line)
	else:
		print("heyy bitch")
		for time_line in time_lines:
			new_lines.append(model_line[:model_line.index(the_bitch)] +
			 time_line.rstrip() + model_line[model_line.index(the_bitch)+len(the_bitch):])


new_file = open("new_interaction_model.txt", "w")
new_lines = "".join(new_lines)
new_file.write(new_lines)
new_file.close()