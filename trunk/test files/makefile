GXX=g++
check=ConfigChecker.o Block.o Ball.o EnergyPill.o Ping.o Simulation.o Robot.o Property.o HazardousObject.o Object.o EnvironmentObject.o RuleKey.o
sim=Strobot.o KickerBot.o  Block.o Ball.o Robot.o EnergyPill.o Property.o HazardousObject.o Object.o EnvironmentObject.o Simulation.o Simulator.o RuleKey.o Ping.o Move.o
crt=BoardCrafter.o Robot.o Property.o HazardousObject.o Object.o EnvironmentObject.o RuleKey.o Block.o Ball.o EnergyPill.o

.PHONY:all
.PHONY:Exceptions

all: checkconf cs229sim create

checkconf: ${check}
	${GXX} ${check} -o checkconf
cs229sim: ${sim}
	${GXX} ${sim} -o cs229sim
create: ${crt}
	${GXX} ${crt} -o create
Simulator.o: Simulator.cpp Strobot.h ConfigException.h Simulation.h RuleKey.h
	${GXX} -c Simulator.cpp
Ball.o: Ball.h Ball.cpp Object.h
	${GXX} -c Ball.cpp
Block.o: Block.h Block.cpp Object.h
	${GXX} -c Block.cpp
BoardCrafter.o: BoardCrafter.cpp ISimObject.h RuleKey.h Hole.h Rock.h Lava.h Water.h Mud.h EnergyPill.h Ball.h Block.h Robot.h Fog.h Jam.h
	${GXX} -c BoardCrafter.cpp
ConfigChecker.o: ConfigChecker.cpp
	${GXX} -c ConfigChecker.cpp
EnergyPill.o: EnergyPill.h EnergyPill.cpp Object.h BadFormatException.h ConfigException.h
	${GXX} -c EnergyPill.cpp
Robot.o: Robot.cpp Robot.h IRobot.h Object.h
	${GXX} -c Robot.cpp
Property.o: Property.cpp Property.h
	${GXX} -c Property.cpp
HazardousObject.o: HazardousObject.cpp HazardousObject.h
	${GXX} -c HazardousObject.cpp
Move.o: Move.h Move.cpp
	${GXX} -c Move.cpp
Object.o: Object.cpp Object.h ISee.h IMove.h IProbe.h ISimParsable.h ISimObject.h
	${GXX} -c Object.cpp
EnvironmentObject.o: EnvironmentObject.cpp EnvironmentObject.h
	${GXX} -c EnvironmentObject.cpp
Ping.o: Ping.h Ping.cpp ISimObject.h ISee.h
	${GXX} -c Ping.cpp
Radar.o: Radar.h Radar.cpp
	${GXX} -c Radar.cpp
RuleKey.o: RuleKey.cpp RuleKey.h
	${GXX} -c RuleKey.cpp
Simulation.o: Simulation.h Rock.h Ball.h Block.h EnergyPill.h  Fog.h Mud.h Move.h Ping.h Radar.h Jam.h Lava.h Water.h Hole.h Property.h Simulation.cpp ISimulation.h ISimParsable.h RuleKey.h
	${GXX} -c Simulation.cpp
Strobot.o: Strobot.cpp Strobot.h Move.h IRobot.h
	${GXX} -c Strobot.cpp
Exceptions: ImmobileError.h UnexpectedAttributeException.h UnmetRequirementException.h BadFormatException.h ConfigException.h

aaa := $(shell ls)
bbb := $(shell ls)

clean:
	rm *.o checkconf cs229sim create
