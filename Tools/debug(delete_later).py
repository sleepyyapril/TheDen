# Made with Python 3.12.9

import psutil

def get_performance_data() -> dict[str:]:
    cpu = psutil.cpu_percent()
    memory = psutil.virtual_memory()
    processes = {p.pid: p.info for p in psutil.process_iter(['name', 'username'])}

    print(f"[DEBUG]\n"
          f"CPU: {cpu:.1f}%\n"
          f"Memory Max: {(memory.total/1048576):.0f}MB\n"
          f"Memory Used: {(memory.used/1048576):.0f}MB\n"
          f"Memory Available: {(memory.free/1048576):.0f}MB\n"
          f"Processes: ")

    print(*sorted(processes.items(), key= lambda x: x[1]["name"]), sep="\n")

    return {
        'cpu': cpu,
        'memory': memory,
        "processes": processes
    }

get_performance_data()
